using System;
using System.Text.Json;
using dms.Api.Messaging;
using Xunit;

public class OcrJobMessageTests
{
    [Fact]
    public void RoundTrip_Serialize_Deserialize_Preserves_Fields()
    {
        var now = DateTimeOffset.UtcNow;
        var msg = new OcrJobMessage(123, "Demo", "/files/demo.pdf", now);

        var json = JsonSerializer.Serialize(msg);
        var back = JsonSerializer.Deserialize<OcrJobMessage>(json);

        Assert.NotNull(back);
        Assert.Equal(msg.DocumentId, back!.DocumentId);
        Assert.Equal(msg.Title, back.Title);
        Assert.Equal(msg.FilePath, back.FilePath);
        Assert.True((msg.UploadedAtUtc - back.UploadedAtUtc).Duration() <= TimeSpan.FromMilliseconds(1));
    }

    [Fact]
    public void Json_Has_Expected_Property_Names()
    {
        var msg = new OcrJobMessage(1, "T", "/p.pdf", DateTimeOffset.Parse("2025-01-01T00:00:00Z"));
        var json = JsonSerializer.Serialize(msg);

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.True(root.TryGetProperty("DocumentId", out _));
        Assert.True(root.TryGetProperty("Title", out _));
        Assert.True(root.TryGetProperty("FilePath", out _));
        Assert.True(root.TryGetProperty("UploadedAtUtc", out _));
    }
}
