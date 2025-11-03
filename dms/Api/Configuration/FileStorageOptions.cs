namespace dms.Api.Configuration;

public class FileStorageOptions
{
	public string Provider { get; set; } = "Minio";
	public string Bucket { get; set; } = "dms-docs";
	public string? PublicBaseUrl { get; set; }
}