using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace dms.Api.Storage;

public sealed record StoredObject(string ObjectKey, string? PublicUrl = null);

public interface IFileStorage
{
    Task<StoredObject> SaveAsync(
        Stream content, string contentType, string objectNameHint, CancellationToken ct = default);

    Task<Stream> GetAsync(string objectKey, CancellationToken ct = default);

    Task<bool> ExistsAsync(string objectKey, CancellationToken ct = default);

    Task DeleteAsync(string objectKey, CancellationToken ct = default);
}
