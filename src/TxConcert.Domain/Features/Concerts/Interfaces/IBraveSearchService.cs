namespace TxConcert.Domain.Features.Concerts;

public interface IBraveSearchService
{
    Task<WebSearchResult> SearchAsync(string query, CancellationToken ct = default);
}

public sealed record WebSearchResult(IReadOnlyList<WebSearchItem> Items);

public sealed record WebSearchItem(
    string Title,
    string Url,
    string Description,
    IReadOnlyList<string> ExtraSnippets);
