namespace dms.Api.Dtos;

public class DocumentUploadDto
{
    public string Title { get; set; } = default!;
    public IFormFile File { get; set; } = default!;
}
