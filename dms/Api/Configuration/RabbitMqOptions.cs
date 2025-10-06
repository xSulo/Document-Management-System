namespace dms.Api.Configuration;

public sealed class RabbitMqOptions
{
    public string HostName { get; init; } = "rabbit";
    public int Port { get; init; } = 5672;
    public string UserName { get; init; } = "dev";
    public string Password { get; init; } = "dev";
    public string Exchange { get; init; } = "dms.exchange";
    public string Queue { get; init; } = "dms.ocr";
    public string RoutingKey { get; init; } = "ocr.new";
}