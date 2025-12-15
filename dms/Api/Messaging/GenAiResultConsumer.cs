using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using dms.Dal.Interfaces;

namespace dms.Api.Messaging
{
    public sealed class GenAiResultConsumer : BackgroundService
    {
        private readonly ILogger<GenAiResultConsumer> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IModel _channel;

        public GenAiResultConsumer(
            ILogger<GenAiResultConsumer> logger,
            IServiceScopeFactory scopeFactory,
            IConnection connection)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;

            _channel = connection.CreateModel();
            _channel.QueueDeclare(
                queue: "genai-result",
                durable: true,
                exclusive: false,
                autoDelete: false
            );
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.Received += async (_, ea) =>
            {
                try
                {
                    var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                    var msg = JsonSerializer.Deserialize<GenAiResultMessage>(json)
                              ?? throw new InvalidOperationException("Invalid genai-result payload");

                    using var scope = _scopeFactory.CreateScope();
                    var repo = scope.ServiceProvider
                        .GetRequiredService<IDocumentRepository>();

                    _logger.LogInformation(
                        "GenAI result received for DocumentId={DocumentId} (SummaryLen={Len})",
                        msg.DocumentId,
                        msg.Summary?.Length ?? 0
                    );

                    await repo.UpdateSummaryAsync(msg.DocumentId, msg.Summary);

                    _channel.BasicAck(ea.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process genai-result message");
                    _channel.BasicNack(
                        ea.DeliveryTag,
                        multiple: false,
                        requeue: true
                    );
                }
            };

            _channel.BasicConsume(
                queue: "genai-result",
                autoAck: false,
                consumer: consumer
            );

            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            try
            {
                _channel?.Close();
                _channel?.Dispose();
            }
            catch
            {
                // ignore shutdown errors
            }

            base.Dispose();
        }
    }
}