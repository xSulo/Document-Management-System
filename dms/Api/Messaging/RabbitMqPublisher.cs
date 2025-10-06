using System.Text.Json;
using dms.Api.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace dms.Api.Messaging;

public sealed class RabbitMqPublisher : IRabbitMqPublisher, IDisposable
{
    private readonly IConnection _conn;
    private readonly IModel _ch;
    private readonly RabbitMqOptions _opt;
    private readonly ILogger<RabbitMqPublisher> _log;

    public RabbitMqPublisher(IOptions<RabbitMqOptions> opt, ILogger<RabbitMqPublisher> log)
    {
        _opt = opt.Value;
        _log = log;

        var factory = new ConnectionFactory
        {
            HostName = _opt.HostName,
            Port = _opt.Port,
            UserName = _opt.UserName,
            Password = _opt.Password,
            DispatchConsumersAsync = true
        };

        _conn = factory.CreateConnection();
        _ch = _conn.CreateModel();

        _ch.ExchangeDeclare(_opt.Exchange, ExchangeType.Topic, durable: true, autoDelete: false);
        _ch.QueueDeclare(_opt.Queue, durable: true, exclusive: false, autoDelete: false);
        _ch.QueueBind(_opt.Queue, _opt.Exchange, _opt.RoutingKey);
    }

    public Task PublishAsync<T>(string routingKey, T payload, CancellationToken ct = default)
    {
        var body = JsonSerializer.SerializeToUtf8Bytes(payload);
        var props = _ch.CreateBasicProperties();
        props.ContentType = "application/json";
        props.DeliveryMode = 2; // persistent

        _ch.BasicPublish(_opt.Exchange, routingKey, props, body);
        _log.LogInformation("Published message to {RoutingKey} ({Bytes} bytes)", routingKey, body.Length);
        return Task.CompletedTask;
    }

    public void Dispose() { _ch?.Dispose(); _conn?.Dispose(); }
}
