using Tesseract;

namespace OcrWorker.Services;

public interface IOcrEngine
{
    Task<string> ExtractTextAsync(string imagePath, string lang, CancellationToken ct);
}

public sealed class TesseractOcr : IOcrEngine
{
    public Task<string> ExtractTextAsync(string imagePath, string lang, CancellationToken ct)
    {
        using var engine = new TesseractEngine(@"./tessdata", lang, EngineMode.Default);
        using var img = Pix.LoadFromFile(imagePath);
        using var page = engine.Process(img);
        return Task.FromResult(page.GetText() ?? string.Empty);
    }
}
