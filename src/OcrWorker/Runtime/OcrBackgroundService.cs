using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OcrWorker.Contracts;
using OcrWorker.Services;
using System.Text.Json;

namespace OcrWorker.Runtime;

public sealed class OcrBackgroundService(
    ILogger<OcrBackgroundService> log,
    IRabbitConsumer bus,
    IObjectStore store,
    IOcrEngine ocr) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        await bus.ConsumeAsync(async body =>
        {
            OcrRequest? msg = JsonSerializer.Deserialize<OcrRequest>(body.Span);
            if (msg is null) return;

            var tmpPdf = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.pdf");
            await store.DownloadAsync(msg.Bucket, msg.ObjectKey, tmpPdf, ct); // MinIO → lokal

            // PDF → PNG (eine Seite reicht für den Proof; bei Bedarf alle Seiten)
            var pngPath = await PdfToPng.ConvertFirstPageAsync(tmpPdf, ct);

            var lang = msg.Language ?? "eng";
            var text = await ocr.ExtractTextAsync(pngPath, lang, ct);

            log.LogInformation("OCR for {DocId}: {Snippet}",
                msg.DocumentId, text.Length > 200 ? text[..200] : text);

            File.Delete(tmpPdf);
            File.Delete(pngPath);
        }, ct);
    }
}
