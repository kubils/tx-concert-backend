using TxConcert.Domain.Common;
using TxConcert.Domain.Common.Base;
using NodaTime;

namespace TxConcert.Domain.Features.Concerts;

public enum ArticleImageSource
{
    None = 0,
    Ticketmaster = 1,
    Spotify = 2,
    Unsplash = 3,
    Pexels = 4,
    Manual = 5
}

public sealed class ConcertArticleEntity : BaseEntity
{
    private ConcertArticleEntity() { } // EF Core

    public static ConcertArticleEntity Create(
        string concertId,
        string title,
        string spot,
        string body,
        IReadOnlyList<string> seoKeywords,
        string metaDescription,
        string? imageUrl,
        string? imageAltText,
        string? imageCredit,
        ArticleImageSource imageSource,
        string aiModel,
        string promptUsed,
        int versionNumber,
        Instant generatedAt,
        int? tokensUsed = null,
        long? generationTimeMs = null)
    {
        var entity = new ConcertArticleEntity
        {
            ConcertId = concertId,
            Title = title,
            Spot = spot,
            Body = body,
            SeoKeywords = seoKeywords.ToList(),
            MetaDescription = metaDescription,
            ImageUrl = imageUrl,
            ImageAltText = imageAltText,
            ImageCredit = imageCredit,
            ImageSource = imageSource,
            AiModel = aiModel,
            PromptUsed = promptUsed,
            VersionNumber = versionNumber,
            IsActive = false,
            GeneratedAt = generatedAt,
            TokensUsed = tokensUsed,
            GenerationTimeMs = generationTimeMs
        };
        entity.SetId(Constants.IdPrefix.ConcertArticle);
        return entity;
    }

    public string ConcertId { get; private set; } = default!;
    public string Title { get; private set; } = default!;
    public string Spot { get; private set; } = default!;
    public string Body { get; private set; } = default!;
    public List<string> SeoKeywords { get; private set; } = [];
    public string MetaDescription { get; private set; } = default!;
    public string? ImageUrl { get; private set; }
    public string? ImageAltText { get; private set; }
    public string? ImageCredit { get; private set; }
    public ArticleImageSource ImageSource { get; private set; }
    public string AiModel { get; private set; } = default!;
    public string PromptUsed { get; private set; } = default!;
    public int VersionNumber { get; private set; }
    public bool IsActive { get; private set; }
    public Instant GeneratedAt { get; private set; }
    public int? TokensUsed { get; private set; }
    public long? GenerationTimeMs { get; private set; }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
