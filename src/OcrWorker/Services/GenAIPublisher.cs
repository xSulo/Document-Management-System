using System;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration; 
using RabbitMQ.Client;

namespace OcrWorker.Services;

public interface IGenAiPublisher
{
    void Publish(long documentId, string text);
}

public class GenAiPublisher : IGenAiPublisher, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly string _exchange;
    private readonly string _routingKey;

    public GenAiPublisher(IConfiguration config)
    {
        var host = config["RabbitMq:HostName"] ?? "rabbit";
        var user = config["RabbitMq:UserName"] ?? "dev";
        var pass = config["RabbitMq:Password"] ?? "dev";
        var port = int.Parse(config["RabbitMq:Port"] ?? "5672");

        _exchange = config["RabbitMq:Exchange"] ?? "dms.exchange";

        _routingKey = "genai";

        var factory = new ConnectionFactory
        {
            HostName = host,
            Port = port,
            UserName = user,
            Password = pass
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.QueueDeclare(queue: _routingKey, durable: true, exclusive: false, autoDelete: false);
    }

    public void Publish(long documentId, string text)
    {
        var payload = new { DocumentId = documentId, Text = text };

        var json = JsonSerializer.Serialize(payload);
        var body = Encoding.UTF8.GetBytes(json);

        _channel.BasicPublish(exchange: "", routingKey: _routingKey, basicProperties: null, body: body);
    }

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
    }
}