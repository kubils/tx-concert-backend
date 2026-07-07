using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TxConcert.Domain.Common.Settings;
using TxConcert.Domain.Features.Concerts;
using TxConcert.Infrastructure.Services.Models;

namespace TxConcert.Infrastructure.Services;

public sealed class BraveSearchService(
    HttpClient httpClient,
    IOptions<BraveSearchSettings> options,
    ILogger<BraveSearchService> logger) : IBraveSearchService
{
    private readonly BraveSearchSettings _settings = options.Value;

    public async Task<WebSearchResult> SearchAsync(string query, CancellationToken ct = default)
    {
        string encodedQuery = WebUtility.UrlEncode(query);
        string url = $"{_settings.BaseUrl}/res/v1/web/search" +
                     $"?q={encodedQuery}" +
                     $"&count={_settings.MaxResultsPerQuery}" +
                     "&extra_snippets=true";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("X-Subscription-Token", _settings.ApiKey);
        request.Headers.Add("Accept", "application/json");

        logger.LogInformation("Brave Search: {Query}", query);

        HttpResponseMessage response = await httpClient.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        string json = await response.Content.ReadAsStringAsync(ct);
        BraveSearchResponse? parsed = JsonSerializer.Deserialize<BraveSearchResponse>(json);

        List<WebSearchItem> items = parsed?.Web?.Results?
            .Where(r => r.Title is not null && r.Description is not null)
            .Select(r => new WebSearchItem(
                r.Title!,
                r.Url ?? "",
                r.Description!,
                r.ExtraSnippets?.AsReadOnly() ?? (IReadOnlyList<string>)[]))
            .ToList() ?? [];

        logger.LogInformation("Brave Search returned {Count} results for: {Query}", items.Count, query);

        return new WebSearchResult(items);
    }
}
