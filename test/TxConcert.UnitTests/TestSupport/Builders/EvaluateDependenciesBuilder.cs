using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NodaTime;
using TxConcert.Application.Features.Concerts.Commands.EvaluateConcertById;
using TxConcert.Application.Features.Concerts.Commands.EvaluateConcerts;
using TxConcert.Domain.Common.Settings;
using TxConcert.Domain.Features.Artists;
using TxConcert.Domain.Features.Concerts;
using TxConcert.Domain.Features.Venues;

namespace TxConcert.UnitTests.TestSupport.Builders;

/// <summary>
/// Shared builder for the 13-14 dependencies that both Evaluate handlers need.
/// Defaults are no-op mocks so tests only configure what they care about.
/// </summary>
public sealed class EvaluateDependenciesBuilder
{
    public Mock<IConcertRepository> ConcertRepo { get; } = new();
    public Mock<IConcertArtistRepository> ConcertArtistRepo { get; } = new();
    public Mock<IArtistRepository> ArtistRepo { get; } = new();
    public Mock<IVenueRepository> VenueRepo { get; } = new();
    public Mock<IConcertDescriptionRepository> DescriptionRepo { get; } = new();
    public Mock<IConcertVibeRepository> VibeRepo { get; } = new();
    public Mock<IArtistVibeProfileRepository> ArtistVibeRepo { get; } = new();
    public Mock<IConcertArticleRepository> ArticleRepo { get; } = new();
    public Mock<IConcertAiEvaluator> Evaluator { get; } = new();
    public Mock<IConcertArticleWriter> ArticleWriter { get; } = new();
    public Mock<ISpotifyService> SpotifyService { get; } = new();
    public IClock Clock { get; } = Mock.Of<IClock>(c =>
        c.GetCurrentInstant() == Instant.FromUtc(2026, 4, 20, 12, 0, 0));
    public IOptions<AiSettings> AiSettings { get; set; } =
        Options.Create(new AiSettings { BatchSize = 10 });

    public EvaluateDependenciesBuilder()
    {
        // Default behaviors: repositories return nothing, article writer returns a valid article.
        DescriptionRepo
            .Setup(r => r.GetActiveForConcertAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ConcertDescriptionEntity?)null);

        DescriptionRepo
            .Setup(r => r.GetNextVersionNumberAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        VibeRepo
            .Setup(r => r.GetByConcertIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ConcertVibeEntity?)null);

        ArtistVibeRepo
            .Setup(r => r.GetByArtistIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ArtistVibeProfileEntity?)null);

        ArticleRepo
            .Setup(r => r.GetActiveForConcertAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ConcertArticleEntity?)null);

        ArticleRepo
            .Setup(r => r.GetNextVersionNumberAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        ConcertArtistRepo
            .Setup(r => r.GetByConcertIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
    }

    public EvaluateDependenciesBuilder WithConcertAndArtist(ConcertEntity concert, ArtistEntity artist, VenueEntity venue)
    {
        ConcertRepo.Setup(r => r.GetByIdAsync(concert.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(concert);

        VenueRepo.Setup(r => r.GetByIdAsync(concert.VenueId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(venue);

        var concertArtist = ConcertArtistEntity.Create(concert.Id, artist.Id, isHeadliner: true, billingOrder: 0);
        ConcertArtistRepo.Setup(r => r.GetByConcertIdAsync(concert.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync([concertArtist]);

        ArtistRepo.Setup(r => r.GetByIdAsync(artist.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(artist);

        return this;
    }

    public EvaluateDependenciesBuilder WithEvaluatorResult(ConcertEvaluationResult result)
    {
        Evaluator.Setup(e => e.EvaluateAsync(It.IsAny<ConcertEvaluationInput>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);
        return this;
    }

    public EvaluateDependenciesBuilder WithArticleResult(ConcertArticleResult result)
    {
        ArticleWriter.Setup(w => w.WriteArticleAsync(It.IsAny<ConcertArticleInput>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);
        return this;
    }

    public EvaluateDependenciesBuilder WithArticleWriterThrows(Exception ex)
    {
        ArticleWriter.Setup(w => w.WriteArticleAsync(It.IsAny<ConcertArticleInput>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(ex);
        return this;
    }

    public EvaluateConcertByIdHandler BuildById() =>
        new(
            ConcertRepo.Object,
            ConcertArtistRepo.Object,
            ArtistRepo.Object,
            VenueRepo.Object,
            DescriptionRepo.Object,
            VibeRepo.Object,
            ArtistVibeRepo.Object,
            ArticleRepo.Object,
            Evaluator.Object,
            ArticleWriter.Object,
            SpotifyService.Object,
            Clock,
            NullLoggerFactory.Instance.CreateLogger<EvaluateConcertByIdHandler>());

    public EvaluateConcertsHandler BuildBatch() =>
        new(
            ConcertRepo.Object,
            ConcertArtistRepo.Object,
            ArtistRepo.Object,
            VenueRepo.Object,
            DescriptionRepo.Object,
            VibeRepo.Object,
            ArtistVibeRepo.Object,
            ArticleRepo.Object,
            Evaluator.Object,
            ArticleWriter.Object,
            SpotifyService.Object,
            Clock,
            AiSettings,
            NullLoggerFactory.Instance.CreateLogger<EvaluateConcertsHandler>());
}
