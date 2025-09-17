namespace dms.Bl.Entities;

public class BlDocument
{
    public long Id { get; set; }
    public string Title { get; set; } = default!;
    public string FilePath { get; set; } = default!;
    public DateTime UploadedAt { get; set; }
}
