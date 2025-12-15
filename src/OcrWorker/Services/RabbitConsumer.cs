using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace OcrWorker.Services;

public interface IRabbitConsumer
{
    Task ConsumeAsync(Func<ReadOnlyMemory<byte>, Task> handler, CancellationToken ct);
}

public sealed class RabbitConsumer : IRabbitConsumer
{
    private readonly IConfiguration _cfg;
    private readonly ILogger<RabbitConsumer> _logger;

    public RabbitConsumer(IConfiguration cfg, ILogger<RabbitConsumer> logger)
    {
        _cfg = cfg;
        _logger = logger;
    }

    public Task ConsumeAsync(Func<ReadOnlyMemory<byte>, Task> handler, CancellationToken ct)
    {
        var host = _cfg["RabbitMq:HostName"]!;
        var port = int.TryParse(_cfg["RabbitMq:Port"], out var p) ? p : 5672;
        var user = _cfg["RabbitMq:UserName"]!;
        var pass = _cfg["RabbitMq:Password"]!;
        var queue = _cfg["RabbitMq:Queue"]!;

        var factory = new RabbitMQ.Client.ConnectionFactory
        {
            HostName = host,
            Port = port,
            UserName = user,
            Password = pass,
            DispatchConsumersAsync = true
        };

        var connection = factory.CreateConnection();
        var channel = connection.CreateModel();
        
        channel.QueueDeclare(queue, durable: true, exclusive: false, autoDelete: false, arguments: null);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.Received += async (_, ea) =>
        {
            try
            {
                await handler(ea.Body);
                channel.BasicAck(ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message from queue {Queue}", queue);
                channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
            }
        };

        channel.BasicConsume(queue: queue, autoAck: false, consumer: consumer);

        ct.Register(() =>
        {
            try { channel.Close(); connection.Close(); } catch { /* ignore */ }
        });

        return Task.CompletedTask;
    }
}
