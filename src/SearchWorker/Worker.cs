using System.Text;
using System.Text.Json;
using Elastic.Clients.Elasticsearch;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace SearchWorker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly ElasticsearchClient _elasticClient;
    private IConnection? _connection;
    private IChannel? _channel;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;

        var settings = new ElasticsearchClientSettings(new Uri("http://elasticsearch:9200"))
            .DefaultIndex("documents");

        _elasticClient = new ElasticsearchClient(settings);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(5000, stoppingToken);

        await InitializeRabbitMqAsync();

        if (_channel == null) return;

        await _channel.QueueDeclareAsync(queue: "dms.search", durable: true, exclusive: false, autoDelete: false, arguments: null);

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            _logger.LogInformation("Received document for indexing.");

            try
            {
                var doc = JsonSerializer.Deserialize<SearchDocument>(message);

                if (doc != null)
                {
                    var response = await _elasticClient.IndexAsync(doc, idx => idx.Index("documents"), stoppingToken);

                    if (response.IsValidResponse)
                    {
                        _logger.LogInformation($"Indexed Document {doc.DocumentId} successfully.");
                        await _channel.BasicAckAsync(ea.DeliveryTag, false);
                    }
                    else
                    {
                        _logger.LogError($"Elasticsearch Error: {response.DebugInformation}");
                        await _channel.BasicNackAsync(ea.DeliveryTag, false, true);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message");
                await _channel.BasicNackAsync(ea.DeliveryTag, false, false);
            }
        };

        await _channel.BasicConsumeAsync(queue: "dms.search", autoAck: false, consumer: consumer);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }

    private async Task InitializeRabbitMqAsync()
    {
        var factory = new ConnectionFactory
        {
            HostName = "rabbit",
            UserName = "dev",
            Password = "dev"
        };

        while (_connection == null)
        {
            try
            {
                _connection = await factory.CreateConnectionAsync();
                _channel = await _connection.CreateChannelAsync();
            }
            catch
            {
                _logger.LogWarning("Waiting for RabbitMQ...");
                await Task.Delay(3000);
            }
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_channel != null) await _channel.CloseAsync();
        if (_connection != null) await _connection.CloseAsync();
        await base.StopAsync(cancellationToken);
    }
}