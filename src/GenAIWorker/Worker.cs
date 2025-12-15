using System;
using System.Text;
using System.Text.Json;
using System.Threading;            
using System.Threading.Tasks;
using GenAIWorker.Contracts;
using GenAIWorker.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace GenAIWorker;

public sealed class Worker : BackgroundService
{
    private readonly ILogger<Worker> _log;
    private readonly IGenAIClient _gemini;
    private readonly RabbitPublisher _publisher;

    public Worker(
        ILogger<Worker> log,
        IGenAIClient gemini,
        RabbitPublisher publisher)
    {
        _log = log;
        _gemini = gemini;
        _publisher = publisher;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = "rabbit",
            Port = 5672,
            UserName = "dev",
            Password = "dev",
            DispatchConsumersAsync = true
        };

        IConnection? connection = null;
        IModel? channel = null;

        while (!stoppingToken.IsCancellationRequested && connection == null)
        {
            try
            {
                connection = factory.CreateConnection();
                channel = connection.CreateModel();
                _log.LogInformation("GenAI Worker connected to RabbitMQ.");
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "RabbitMQ not ready. Retrying in 3s...");

                await Task.Delay(3000, stoppingToken);
            }
        }

        if (connection == null || channel == null) return;

        channel.QueueDeclare(
            queue: "genai",
            durable: true,
            exclusive: false,
            autoDelete: false
        );

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.Received += async (_, ea) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                var msg = JsonSerializer.Deserialize<GenAIRequest>(json)!;

                _log.LogInformation("GenAI received for document {Id}", msg.DocumentId);

                var result = await _gemini.ProcessAsync(msg, stoppingToken);

                await _publisher.PublishAsync(result, stoppingToken);

                channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "GenAI failed – requeue");
                channel.BasicNack(ea.DeliveryTag, false, true);
            }
        };

        channel.BasicConsume("genai", false, consumer);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}