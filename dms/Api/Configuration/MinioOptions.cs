namespace dms.Api.Configuration;

public class MinioOptions
{
    public string Endpoint { get; set; } = "minio:9000";
    public string AccessKey { get; set; } = default!;
    public string SecretKey { get; set; } = default!;
    public bool UseSsl { get; set; }
}