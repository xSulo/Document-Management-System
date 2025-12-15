using System;
using System.Text.Json.Serialization;

namespace SearchWorker;

public class SearchDocument
{
    [JsonPropertyName("DocumentId")]
    public long DocumentId { get; set; }

    [JsonPropertyName("Title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("Text")]
    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}