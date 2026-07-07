namespace TxConcert.Domain.Features.Concerts;

public interface IConcertArticleWriter
{
    Task<ConcertArticleResult> WriteArticleAsync(ConcertArticleInput input, CancellationToken ct = default);
}

public sealed record ConcertArticleInput
{
    public required string ConcertName { get; init; }
    public required string EventDate { get; init; }
    public string? StartTime { get; init; }
    public string? VenueName { get; init; }
    public string? VenueCity { get; init; }
    public string? GenreName { get; init; }
    public string? SubGenreName { get; init; }
    public decimal? PriceMin { get; init; }
    public decimal? PriceMax { get; init; }
    public string? Info { get; init; }
    public string? PleaseNote { get; init; }
    public IReadOnlyList<ArticleArtistInput> Artists { get; init; } = [];

    // Carry-over from evaluator: already-written description + vibe summary to avoid re-research
    public string? EvaluatorDescription { get; init; }
    public string? EvaluatorVibeSummary { get; init; }

    // Image candidates: writer picks which source to use.
    public string? TicketmasterImageUrl { get; init; }
}

public sealed record ArticleArtistInput
{
    public required string Name { get; init; }
    public string? GenreName { get; init; }
    public string? Bio { get; init; }
    public bool IsHeadliner { get; init; }
    public string? SpotifyImageUrl { get; init; }
    public string? ArtistImageUrl { get; init; }
}

public sealed record ConcertArticleResult
{
    public required string Title { get; init; }
    public required string Spot { get; init; }
    public required string Body { get; init; }
    public IReadOnlyList<string> SeoKeywords { get; init; } = [];
    public required string MetaDescription { get; init; }
    public string? ImageUrl { get; init; }
    public string? ImageAltText { get; init; }
    public string? ImageCredit { get; init; }
    public ArticleImageSource ImageSource { get; init; }
    public required string AiModel { get; init; }
    public int? TokensUsed { get; init; }
    public long? GenerationTimeMs { get; init; }
}
