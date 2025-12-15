using System;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace OcrWorker.Services;

public interface IGenAiPublisher
{
    void Publish(long documentId, string text, string title);
}

public class GenAiPublisher : IGenAiPublisher, IDisposable
{
    private readonly ILogger<GenAiPublisher> _logger;
    private readonly string _host;
    private readonly int _port;
    private readonly string _user;
    private readonly string _pass;

    private const string QueueGenAi = "genai";
    private const string QueueSearch = "dms.search";

    private IConnection? _connection;
    private IModel? _channel;

    public GenAiPublisher(IConfiguration config, ILogger<GenAiPublisher> logger)
    {
        _logger = logger;

        _host = config["RabbitMq:HostName"] ?? "rabbit";
        _port = int.Parse(config["RabbitMq:Port"] ?? "5672");
        _user = config["RabbitMq:UserName"] ?? "dev";
        _pass = config["RabbitMq:Password"] ?? "dev";

        InitializeRabbitMq();
    }

    private void InitializeRabbitMq()
    {
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _host,
                Port = _port,
                UserName = _user,
                Password = _pass,
                AutomaticRecoveryEnabled = true
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(queue: QueueGenAi, durable: true, exclusive: false, autoDelete: false);

            _channel.QueueDeclare(queue: QueueSearch, durable: true, exclusive: false, autoDelete: false);

            _logger.LogInformation("RabbitMQ connection established and queues ({Q1}, {Q2}) declared.", QueueGenAi, QueueSearch);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Failed to initialize RabbitMQ connection.");
            throw;
        }
    }

    public void Publish(long documentId, string text, string title)
    {
        if (_channel == null || _channel.IsClosed)
        {
            _logger.LogWarning("RabbitMQ channel is closed. Attempting to reconnect...");
            InitializeRabbitMq();
        }

        var payload = new
        {
            DocumentId = documentId,
            Text = text,
            Title = title,
            Timestamp = DateTime.UtcNow 
        };

        var json = JsonSerializer.Serialize(payload);
        var body = Encoding.UTF8.GetBytes(json);

        var properties = _channel!.CreateBasicProperties();
        properties.Persistent = true;

        _channel.BasicPublish(exchange: "",
                              routingKey: QueueGenAi,
                              basicProperties: properties,
                              body: body);

        _channel.BasicPublish(exchange: "",
                              routingKey: QueueSearch,
                              basicProperties: properties,
                              body: body);

        _logger.LogInformation("Published DocumentId {DocId} to queues '{Q1}' and '{Q2}'. Payload size: {Bytes} bytes",
            documentId, QueueGenAi, QueueSearch, body.Length);
    }

    public void Dispose()
    {
        try
        {
            _channel?.Close();
            _connection?.Close();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while closing RabbitMQ connection.");
        }
    }
}