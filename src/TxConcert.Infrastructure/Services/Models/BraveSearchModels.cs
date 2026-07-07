using System.Text.Json.Serialization;

namespace TxConcert.Infrastructure.Services.Models;

internal sealed class BraveSearchResponse
{
    [JsonPropertyName("web")]
    public BraveWebResults? Web { get; set; }
}

internal sealed class BraveWebResults
{
    [JsonPropertyName("results")]
    public List<BraveWebResult>? Results { get; set; }
}

internal sealed class BraveWebResult
{
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("extra_snippets")]
    public List<string>? ExtraSnippets { get; set; }
}
