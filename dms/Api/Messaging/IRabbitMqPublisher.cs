using System.Threading;
using System.Threading.Tasks;

namespace dms.Api.Messaging;
public interface IRabbitMqPublisher : System.IDisposable
{
    Task PublishAsync<T>(string routingKey, T payload, CancellationToken ct = default);
}
