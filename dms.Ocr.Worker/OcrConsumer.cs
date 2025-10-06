using System.Text.Json;
using dms.Ocr.Worker.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace dms.Ocr.Worker;

public sealed class OcrConsumer : BackgroundService
{
    private readonly ILogger<OcrConsumer> _log;
    private readonly RabbitMqOptions _opt;
    private IConnection? _conn;
    private IModel? _ch;

    public OcrConsumer(IOptions<RabbitMqOptions> opt, ILogger<OcrConsumer> log)
    { _opt = opt.Value; _log = log; }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
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
        _ch.QueueDeclare(_opt.Queue, durable: true, exclusive: false, autoDelete: false);
        _ch.BasicQos(0, 1, false);

        var consumer = new AsyncEventingBasicConsumer(_ch);
        consumer.Received += async (_, ea) =>
        {
            try
            {
                var msg = JsonSerializer.Deserialize<OcrJobMessage>(ea.Body.Span);
                _log.LogInformation("OCR worker received: DocumentId={Id}, Title={Title}, Path={Path}",
                    msg?.DocumentId, msg?.Title, msg?.FilePath);

                // TODO: später OCR implementieren
                _ch!.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error processing message");
                _ch!.BasicNack(ea.DeliveryTag, false, requeue: true);
            }
            await Task.CompletedTask;
        };

        _ch.BasicConsume(_opt.Queue, autoAck: false, consumer);
        return Task.CompletedTask;
    }

    public override void Dispose() { _ch?.Dispose(); _conn?.Dispose(); base.Dispose(); }
}

public record OcrJobMessage(long DocumentId, string Title, string FilePath, DateTimeOffset UploadedAtUtc);
