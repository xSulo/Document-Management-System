namespace dms.Models;

public class Document
{
    public long Id { get; set; }
    public string? Title { get; set; }
    public string? FilePath { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}
