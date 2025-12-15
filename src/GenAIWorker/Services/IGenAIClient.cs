using System.Threading;
using System.Threading.Tasks;
using GenAIWorker.Contracts;

namespace GenAIWorker.Services;

public interface IGenAIClient
{
    Task<GenAIResult> ProcessAsync(GenAIRequest request, CancellationToken ct);
}