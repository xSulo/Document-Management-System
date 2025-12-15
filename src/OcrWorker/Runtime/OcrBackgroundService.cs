using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OcrWorker.Contracts;
using OcrWorker.Services;
using System.Text.Json;
using OcrWorker.Services;

namespace OcrWorker.Runtime;

public sealed class OcrBackgroundService(
    ILogger<OcrBackgroundService> log,
    IRabbitConsumer bus,
    IObjectStore store,
    IOcrEngine ocr,
    IGenAiPublisher genAiPublisher
    ) : BackgroundService
{
    private const string BucketName = "dms-docs";

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        await bus.ConsumeAsync(async body =>
        {
            OcrRequest? msg = JsonSerializer.Deserialize<OcrRequest>(body.Span);
            if (msg is null) return;

            log.LogInformation("Processing DocumentId: {Id} from File: {File}", msg.DocumentId, msg.FilePath);

            var tmpPdf = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.pdf");
            
            try
            {
                await store.DownloadAsync(BucketName, msg.FilePath, tmpPdf, ct);

                var pngPath = await PdfToPng.ConvertFirstPageAsync(tmpPdf, ct);
                var text = await ocr.ExtractTextAsync(pngPath, "eng", ct);

                log.LogInformation("OCR done for {Id}. Text length: {Len}", msg.DocumentId, text.Length);

                genAiPublisher.Publish(msg.DocumentId, text, msg.Title);
                log.LogInformation("-> Sent to GenAI Queue.");

                if (File.Exists(pngPath)) File.Delete(pngPath);
            }
            finally
            {
                if (File.Exists(tmpPdf)) File.Delete(tmpPdf);
            }
        }, ct);
    }
}
