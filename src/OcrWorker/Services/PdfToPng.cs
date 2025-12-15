using System.Diagnostics;
using System.IO; 

namespace OcrWorker.Services;

public static class PdfToPng
{
    public static async Task<string> ConvertFirstPageAsync(string pdfPath, CancellationToken ct)
    {
        var outPng = Path.Combine(Path.GetTempPath(), $"{Path.GetFileNameWithoutExtension(pdfPath)}.png");

        var gsPath = "/usr/bin/gs";

        if (!File.Exists(gsPath)) gsPath = "gs";

        var args = $"-dQUIET -dSAFER -dBATCH -dNOPAUSE -sDEVICE=png16m -r300 -dFirstPage=1 -dLastPage=1 " +
                   $"-sOutputFile=\"{outPng}\" \"{pdfPath}\"";

        var psi = new ProcessStartInfo(gsPath, args)
        {
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false, 
            CreateNoWindow = true
        };

        using var p = Process.Start(psi);
        if (p == null) throw new InvalidOperationException("Failed to start Ghostscript process.");

        await p.WaitForExitAsync(ct);

        if (p.ExitCode != 0)
        {
            var error = await p.StandardError.ReadToEndAsync(ct);
            throw new InvalidOperationException($"Ghostscript failed with exit code {p.ExitCode}: {error}");
        }

        if (!File.Exists(outPng))
            throw new InvalidOperationException($"Ghostscript did not create the output file at {outPng}");

        return outPng;
    }
}