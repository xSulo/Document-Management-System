using System.Diagnostics;

namespace OcrWorker.Services;

public static class PdfToPng
{
    public static async Task<string> ConvertFirstPageAsync(string pdfPath, CancellationToken ct)
    {
        var outPng = Path.Combine(Path.GetTempPath(), $"{Path.GetFileNameWithoutExtension(pdfPath)}.png");
        // Ghostscript: -dFirstPage=1 -dLastPage=1 → erste Seite
        var args = $"-dQUIET -dSAFER -dBATCH -dNOPAUSE -sDEVICE=png16m -r300 -dFirstPage=1 -dLastPage=1 " +
                   $"-sOutputFile=\"{outPng}\" \"{pdfPath}\"";

        var psi = new ProcessStartInfo("gs", args)
        {
            RedirectStandardError = true,
            RedirectStandardOutput = true
        };
        using var p = Process.Start(psi)!;
        await p.WaitForExitAsync(ct);
        if (!File.Exists(outPng)) throw new InvalidOperationException("Ghostscript failed.");
        return outPng;
    }
}
