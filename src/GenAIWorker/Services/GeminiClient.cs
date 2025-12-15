using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using GenAIWorker.Contracts;

namespace GenAIWorker.Services;

public sealed class GeminiClient : IGenAIClient
{
    private readonly HttpClient _http;

    public GeminiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<GenAIResult> ProcessAsync(GenAIRequest request, CancellationToken ct)
    {
        var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY");
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("Missing env var GEMINI_API_KEY");

        var model = Environment.GetEnvironmentVariable("GEMINI_MODEL") ?? "gemini-2.0-flash-lite-001";

        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";

        var prompt =
            "Erstelle eine kurze, präzise Zusammenfassung (max. 6 Bulletpoints und in der Originalsprache) vom folgenden Text:\n\n" +
            request.Text;

        var payload = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[] { new { text = prompt } }
                }
            }
        };

        var sw = Stopwatch.StartNew();

        using var resp = await _http.PostAsJsonAsync(url, payload, ct);
        var json = await resp.Content.ReadAsStringAsync(ct);

        if (!resp.IsSuccessStatusCode)
            throw new InvalidOperationException($"Gemini API error {(int)resp.StatusCode}: {json}");

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        string summary =
            root.GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString()
            ?? string.Empty;

        sw.Stop();

        return new GenAIResult(
            DocumentId: request.DocumentId,
            Summary: summary,
            Model: model,
            ProcessingTimeMs: (int)sw.ElapsedMilliseconds
        );
    }
}