namespace TxConcert.Infrastructure.Services.Models;

internal sealed class ClaudeResponse
{
    public List<ClaudeContent>? Content { get; set; }
    public ClaudeUsage? Usage { get; set; }
}

internal sealed class ClaudeContent
{
    public string? Type { get; set; }
    public string? Text { get; set; }
}

internal sealed class ClaudeUsage
{
    public int? InputTokens { get; set; }
    public int? OutputTokens { get; set; }
}

internal sealed class AiEvaluationResponse
{
    public string? Description { get; set; }
    public AiConcertVibes? ConcertVibes { get; set; }
    public List<AiArtistVibe>? ArtistVibes { get; set; }
}

internal sealed class AiConcertVibes
{
    public int Energy { get; set; }
    public int SoundQuality { get; set; }
    public int Hype { get; set; }
    public int CrowdVibe { get; set; }
    public int ValueForMoney { get; set; }
    public string? Summary { get; set; }
}

internal sealed class AiArtistVibe
{
    public string? ArtistName { get; set; }
    public int Visuals { get; set; }
    public int Sound { get; set; }
    public int Energy { get; set; }
    public int FanInteraction { get; set; }
    public int TexasSpirit { get; set; }
    public string? Summary { get; set; }
}

internal sealed class AiArticleResponse
{
    public string? Title { get; set; }
    public string? Spot { get; set; }
    public string? Body { get; set; }
    public List<string>? SeoKeywords { get; set; }
    public string? MetaDescription { get; set; }
    public string? ChosenImageSource { get; set; }
    public string? ImageAltText { get; set; }
}
