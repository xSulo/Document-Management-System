using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using dms.Api.Configuration;
using dms.Api.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

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

        try
        {
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

            _log.LogInformation("RabbitMQ connection established successfully.");
        }
        catch (BrokerUnreachableException ex)
        {
            _log.LogError(ex, "RabbitMQ broker unreachable during startup.");
            throw new QueueUnavailableException("Failed to connect to RabbitMQ broker.", ex);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Unexpected error while initializing RabbitMQ connection.");
            throw new QueueUnavailableException("Error initializing RabbitMQ connection.", ex);
        }
    }

    public Task PublishAsync<T>(string routingKey, T payload, CancellationToken ct = default)
    {
        try
        {
            var body = JsonSerializer.SerializeToUtf8Bytes(payload);
            var props = _ch.CreateBasicProperties();
            props.ContentType = "application/json";
            props.DeliveryMode = 2; // persistent

            _ch.BasicPublish(_opt.Exchange, routingKey, props, body);
            _log.LogInformation("Published message to {RoutingKey} ({Bytes} bytes)", routingKey, body.Length);

            return Task.CompletedTask;
        }
        catch (AlreadyClosedException ex)
        {
            _log.LogError(ex, "RabbitMQ channel or connection already closed.");
            throw new QueueUnavailableException("RabbitMQ channel closed unexpectedly.", ex);
        }
        catch (BrokerUnreachableException ex)
        {
            _log.LogError(ex, "RabbitMQ broker unreachable while publishing message.");
            throw new QueueUnavailableException("Unable to reach RabbitMQ broker.", ex);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Unexpected error while publishing message to RabbitMQ.");
            throw new QueueUnavailableException("Error while publishing message to RabbitMQ.", ex);
        }
    }

    public void Dispose()
    {
        _ch?.Dispose();
        _conn?.Dispose();
    }
}
