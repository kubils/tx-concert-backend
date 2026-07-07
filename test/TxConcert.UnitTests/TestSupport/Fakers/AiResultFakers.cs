using TxConcert.Domain.Features.Concerts;

namespace TxConcert.UnitTests.TestSupport.Fakers;

public static class AiResultFakers
{
    public static ConcertEvaluationResult EvaluationResult(
        string description = "A stellar Texas night of rock.",
        string aiModel = "claude-sonnet-4",
        int score = 8,
        IReadOnlyList<string>? artistNames = null)
    {
        return new ConcertEvaluationResult
        {
            Description = description,
            AiModel = aiModel,
            ConcertVibes = new ConcertVibeScores
            {
                Energy = score,
                SoundQuality = score,
                Hype = score,
                CrowdVibe = score,
                ValueForMoney = score,
                Summary = "High-energy event."
            },
            ArtistVibes = (artistNames ?? ["The Headliner"]).Select(n => new ArtistVibeResult
            {
                ArtistName = n,
                Visuals = score,
                Sound = score,
                Energy = score,
                FanInteraction = score,
                TexasSpirit = score,
                Summary = "Solid stage presence."
            }).ToList()
        };
    }

    public static ConcertArticleResult ArticleResult(
        string title = "Texas Lights Up for a Night of Rock",
        string aiModel = "claude-sonnet-4",
        ArticleImageSource imageSource = ArticleImageSource.Spotify)
    {
        return new ConcertArticleResult
        {
            Title = title,
            Spot = "Rock legends converge in Austin for an unforgettable show.",
            Body = "Paragraph one.\n\nParagraph two.",
            SeoKeywords = ["rock", "texas", "live music"],
            MetaDescription = "A Rolling-Stone-worthy account of the night.",
            ImageUrl = "https://img/spotify.jpg",
            ImageAltText = "Band on stage",
            ImageCredit = "Photo via Spotify API",
            ImageSource = imageSource,
            AiModel = aiModel,
            TokensUsed = 1234,
            GenerationTimeMs = 1500
        };
    }
}
