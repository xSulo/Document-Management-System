using System;
using System.Threading.Tasks;
using Elastic.Clients.Elasticsearch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;

namespace dms.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    private readonly ElasticsearchClient _elasticClient;
    private readonly ILogger<SearchController> _logger;

    public SearchController(ElasticsearchClient elasticClient, ILogger<SearchController> logger)
    {
        _elasticClient = elasticClient;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return Ok(Array.Empty<object>());
        }

        try
        {
            var response = await _elasticClient.SearchAsync<SearchDocument>(s => s
                .Index("documents") 
                .Query(q => q
                    .MultiMatch(m => m
                        .Query(query)
                        .Fields("*")
                        .Fuzziness(new Fuzziness("AUTO")) 
                    )
                )
            );

            if (!response.IsValidResponse)
            {
                _logger.LogError("Elasticsearch failed: {Debug}", response.DebugInformation);
                return StatusCode(500, "Search unavailable");
            }

            var results = response.Documents.Select(doc => new
            {
                doc.DocumentId,
                doc.Title,
                Snippet = doc.Content.Length > 100 ? doc.Content.Substring(0, 100) + "..." : doc.Content
            });

            return Ok(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Search failed");
            return StatusCode(500, "Internal Server Error");
        }
    }

    [HttpGet("debug")]
    public async Task<IActionResult> Debug()
    {
        var response = await _elasticClient.SearchAsync<object>(s => s
            .Index("documents")
            .Query(q => q.MatchAll())
        );

        return Ok(response.Documents);
    }
}

public class SearchDocument
{
    [JsonPropertyName("DocumentId")]
    public long DocumentId { get; set; }

    [JsonPropertyName("Title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("Content")] 
    public string Content { get; set; } = string.Empty;
}