using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using GenAIWorker.Contracts;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace GenAIWorker.Services;

public sealed class RabbitPublisher : IDisposable
{
    private readonly ILogger<RabbitPublisher> _log;

    private IConnection? _connection;
    private IModel? _channel;

    private const string ResultQueue = "genai-result";

    public RabbitPublisher(ILogger<RabbitPublisher> log)
    {
        _log = log;
    }

    private void EnsureConnected()
    {
        if (_connection is { IsOpen: true } && _channel is { IsOpen: true })
            return;

        var host = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "rabbit";
        var user = Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? "dev";
        var pass = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") ?? "dev";
        var port = int.Parse(Environment.GetEnvironmentVariable("RABBITMQ_PORT") ?? "5672");

        _log.LogInformation("Connecting to RabbitMQ at {Host}:{Port}", host, port);

        var factory = new ConnectionFactory
        {
            HostName = host,
            Port = port,
            UserName = user,
            Password = pass,
            DispatchConsumersAsync = true
        };

        // Retry-Loop (Docker-safe)
        for (var i = 1; i <= 10; i++)
        {
            try
            {
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                _channel.QueueDeclare(
                    queue: ResultQueue,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null
                );

                _log.LogInformation("RabbitPublisher connected. Queue={Queue}", ResultQueue);
                return;
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "RabbitMQ not ready (attempt {Attempt}/10)", i);
                Thread.Sleep(3000);
            }
        }

        throw new InvalidOperationException("RabbitMQ connection failed after retries.");
    }

    public Task PublishAsync(GenAIResult result, CancellationToken ct)
    {
        EnsureConnected();

        var json = JsonSerializer.Serialize(result);
        var body = Encoding.UTF8.GetBytes(json);

        var props = _channel!.CreateBasicProperties();
        props.Persistent = true;

        _channel.BasicPublish(
            exchange: "",
            routingKey: ResultQueue,
            basicProperties: props,
            body: body
        );

        _log.LogInformation(
            "GenAI result published. DocumentId={Id}, Length={Len}",
            result.DocumentId,
            result.Summary?.Length ?? 0
        );

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        try
        {
            _channel?.Close();
            _connection?.Close();
        }
        catch
        {
            // ignore shutdown errors
        }
    }
}