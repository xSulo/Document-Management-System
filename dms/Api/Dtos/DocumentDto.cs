namespace dms.Api.Dtos;

public class DocumentDto
{
    public long Id { get; set; }
    public string Title { get; set; } = default!;
    public string FilePath { get; set; } = default!;
    public DateTime UploadedAt { get; set; }
    public string? Summary { get; set; }
}
