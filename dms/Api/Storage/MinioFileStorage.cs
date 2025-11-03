using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;        
using Microsoft.Extensions.Options;         
using Minio;                               
using Minio.DataModel.Args;                 
using dms.Api.Configuration;               

namespace dms.Api.Storage;

public class MinioFileStorage : IFileStorage
{
    private readonly ILogger<MinioFileStorage> _log;
    private readonly IOptions<FileStorageOptions> _fsOpt;
    private readonly IMinioClient _client;

    public MinioFileStorage(
        ILogger<MinioFileStorage> log,
        IOptions<FileStorageOptions> fsOpt,
        IMinioClient client)
    {
        _log = log;
        _fsOpt = fsOpt;
        _client = client;
    }

    private async Task EnsureBucketAsync(string bucket, CancellationToken ct)
    {
        var exists = await _client.BucketExistsAsync(new BucketExistsArgs().WithBucket(bucket), ct);
        if (!exists)
        {
            _log.LogInformation("Creating bucket {Bucket}", bucket);
            await _client.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucket), ct);
        }
    }

    public async Task<StoredObject> SaveAsync(
        Stream content, string contentType, string objectNameHint, CancellationToken ct = default)
    {
        var bucket = _fsOpt.Value.Bucket;
        await EnsureBucketAsync(bucket, ct);

        var baseName = string.Concat(objectNameHint.Where(char.IsLetterOrDigit));
        if (string.IsNullOrWhiteSpace(baseName)) baseName = "document";
        var objectKey = $"{baseName}_{Guid.NewGuid():N}.pdf";

        var put = new PutObjectArgs()
            .WithBucket(bucket)
            .WithObject(objectKey)
            .WithStreamData(content)
            .WithObjectSize(content.Length)      
            .WithContentType(contentType);

        await _client.PutObjectAsync(put, ct);

        string? publicUrl = null;
        var baseUrl = _fsOpt.Value.PublicBaseUrl;
        if (!string.IsNullOrWhiteSpace(baseUrl))
            publicUrl = $"{baseUrl!.TrimEnd('/')}/{bucket}/{objectKey}";

        _log.LogInformation("Object stored in MinIO: {Bucket}/{Key}", bucket, objectKey);
        return new StoredObject(objectKey, publicUrl);
    }

    public async Task<Stream> GetAsync(string objectKey, CancellationToken ct = default)
    {
        var ms = new MemoryStream();
        await _client.GetObjectAsync(
            new GetObjectArgs()
                .WithBucket(_fsOpt.Value.Bucket)
                .WithObject(objectKey)
                .WithCallbackStream(s => s.CopyTo(ms)),
            ct);
        ms.Position = 0;
        return ms;
    }

    public async Task<bool> ExistsAsync(string objectKey, CancellationToken ct = default)
    {
        try
        {
            await _client.StatObjectAsync(
                new StatObjectArgs()
                    .WithBucket(_fsOpt.Value.Bucket)
                    .WithObject(objectKey),
                ct);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task DeleteAsync(string objectKey, CancellationToken ct = default)
    {
        await _client.RemoveObjectAsync(
            new RemoveObjectArgs()
                .WithBucket(_fsOpt.Value.Bucket)
                .WithObject(objectKey),
            ct);
    }
}
