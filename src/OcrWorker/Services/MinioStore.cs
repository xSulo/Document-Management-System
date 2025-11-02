using Minio;
using Minio.DataModel.Args;
using Microsoft.Extensions.Configuration;

namespace OcrWorker.Services;

public interface IObjectStore
{
    Task DownloadAsync(string bucket, string objectKey, string localPath, CancellationToken ct);
}

public sealed class MinioStore : IObjectStore
{
    private readonly IMinioClient _client;

    public MinioStore(IConfiguration cfg)
    {
        var endpoint = cfg["Minio:Endpoint"]!;
        var key = cfg["Minio:AccessKey"]!;
        var secret = cfg["Minio:SecretKey"]!;
        var useSsl = bool.Parse(cfg["Minio:UseSSL"] ?? "false");

        _client = new MinioClient()
            .WithEndpoint(endpoint)
            .WithCredentials(key, secret)
            .WithSSL(useSsl)
            .Build();
    }

    public async Task DownloadAsync(string bucket, string objectKey, string localPath, CancellationToken ct)
    {
        var args = new GetObjectArgs()
            .WithBucket(bucket)
            .WithObject(objectKey)
            .WithCallbackStream(stream =>
            {
                using var fs = File.Create(localPath);
                stream.CopyTo(fs);
            });

        await _client.GetObjectAsync(args, ct); // <- CT hier übergeben
    }
}
