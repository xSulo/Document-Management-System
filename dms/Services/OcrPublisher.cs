using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

public class OcrPublisher
{
    private readonly ConnectionFactory _factory;
    private readonly string _exchange;
    private readonly string _routingKey;

    public OcrPublisher(IConfiguration config)
    {
        _factory = new ConnectionFactory
        {
            HostName = config["RabbitMq:HostName"],
            Port = int.Parse(config["RabbitMq:Port"] ?? "5672"),
            UserName = config["RabbitMq:UserName"],
            Password = config["RabbitMq:Password"]
        };
        _exchange = config["RabbitMq:Exchange"]!;
        _routingKey = config["RabbitMq:RoutingKey"]!;
    }

    public void Publish(Guid documentId, string bucket, string objectKey, string language = "eng")
    {
        using var connection = _factory.CreateConnection();
        using var channel = connection.CreateModel();

        var message = new
        {
            documentId,
            bucket,
            objectKey,
            language
        };

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

        channel.BasicPublish(
            exchange: _exchange,
            routingKey: _routingKey,
            basicProperties: null,
            body: body
        );
    }
}
