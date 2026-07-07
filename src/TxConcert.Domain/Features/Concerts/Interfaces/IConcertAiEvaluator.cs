namespace TxConcert.Domain.Features.Concerts;

public interface IConcertAiEvaluator
{
    Task<ConcertEvaluationResult> EvaluateAsync(ConcertEvaluationInput input, CancellationToken ct = default);
}

public sealed record ConcertEvaluationInput
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
    public IReadOnlyList<ArtistEvaluationInput> Artists { get; init; } = [];
    public Dictionary<string, string> WebResearch { get; init; } = new();
}

public sealed record ArtistEvaluationInput
{
    public required string Name { get; init; }
    public string? GenreName { get; init; }
    public string? Bio { get; init; }
    public string? SpotifyUrl { get; init; }
    public bool IsHeadliner { get; init; }

    /// <summary>
    /// When true, the artist already has an AI-generated vibe profile.
    /// AI evaluation and web research for this artist's vibes will be skipped to save tokens.
    /// </summary>
    public bool HasExistingVibeProfile { get; init; }
}

public sealed record ConcertEvaluationResult
{
    public required string Description { get; init; }
    public required string AiModel { get; init; }
    public required ConcertVibeScores ConcertVibes { get; init; }
    public IReadOnlyList<ArtistVibeResult> ArtistVibes { get; init; } = [];
}

public sealed record ConcertVibeScores
{
    public int Energy { get; init; }
    public int SoundQuality { get; init; }
    public int Hype { get; init; }
    public int CrowdVibe { get; init; }
    public int ValueForMoney { get; init; }
    public string? Summary { get; init; }
}

public sealed record ArtistVibeResult
{
    public required string ArtistName { get; init; }
    public int Visuals { get; init; }
    public int Sound { get; init; }
    public int Energy { get; init; }
    public int FanInteraction { get; init; }
    public int TexasSpirit { get; init; }
    public string? Summary { get; init; }
}
