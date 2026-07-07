using FluentAssertions;
using Moq;
using NodaTime;
using TxConcert.Application.Features.Concerts.Commands.EvaluateConcerts;
using TxConcert.Domain.Common.Enums;
using TxConcert.Domain.Features.Concerts;
using TxConcert.UnitTests.TestSupport.Builders;
using TxConcert.UnitTests.TestSupport.Fakers;

namespace TxConcert.UnitTests.Application.Concerts;

public class EvaluateConcertsHandlerTests
{
    [Fact]
    public async Task Handle_NoUnevaluatedConcerts_ExitsEarlyWithZeroCounts()
    {
        // Arrange
        EvaluateDependenciesBuilder deps = new();
        deps.ConcertRepo
            .Setup(r => r.GetUnevaluatedAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        EvaluateConcertsHandler handler = deps.BuildBatch();

        // Act
        EvaluateConcertsResult result = await handler.Handle(new EvaluateConcertsCommand(), CancellationToken.None);

        // Assert
        result.Evaluated.Should().Be(0);
        result.Skipped.Should().Be(0);
        result.Failed.Should().Be(0);
        deps.Evaluator.Verify(
            e => e.EvaluateAsync(It.IsAny<ConcertEvaluationInput>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_MultipleConcerts_CallsEvaluatorForEach()
    {
        // Arrange
        var venue = DomainEntityFakers.Venue();
        var artist = DomainEntityFakers.Artist("The Headliner");
        var concert1 = DomainEntityFakers.Concert(venueId: venue.Id);
        var concert2 = DomainEntityFakers.Concert(venueId: venue.Id);

        EvaluateDependenciesBuilder deps = new();
        deps.ConcertRepo
            .Setup(r => r.GetUnevaluatedAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([concert1, concert2]);

        deps.WithConcertAndArtist(concert1, artist, venue);
        deps.WithConcertAndArtist(concert2, artist, venue);
        deps.WithEvaluatorResult(AiResultFakers.EvaluationResult(artistNames: ["The Headliner"]));
        deps.WithArticleResult(AiResultFakers.ArticleResult());

        EvaluateConcertsHandler handler = deps.BuildBatch();

        // Act
        EvaluateConcertsResult result = await handler.Handle(new EvaluateConcertsCommand(), CancellationToken.None);

        // Assert
        result.Evaluated.Should().Be(2);
        result.Failed.Should().Be(0);
        deps.Evaluator.Verify(
            e => e.EvaluateAsync(It.IsAny<ConcertEvaluationInput>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_EvaluatorThrowsForOneConcert_LogsAndContinuesToNext()
    {
        // Arrange
        var venue = DomainEntityFakers.Venue();
        var artist = DomainEntityFakers.Artist("The Headliner");
        var badConcert = DomainEntityFakers.Concert(venueId: venue.Id);
        var goodConcert = DomainEntityFakers.Concert(venueId: venue.Id);

        EvaluateDependenciesBuilder deps = new();
        deps.ConcertRepo
            .Setup(r => r.GetUnevaluatedAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([badConcert, goodConcert]);

        deps.WithConcertAndArtist(badConcert, artist, venue);
        deps.WithConcertAndArtist(goodConcert, artist, venue);

        // First call throws, second call succeeds
        deps.Evaluator
            .SetupSequence(e => e.EvaluateAsync(It.IsAny<ConcertEvaluationInput>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("AI failed"))
            .ReturnsAsync(AiResultFakers.EvaluationResult(artistNames: ["The Headliner"]));

        deps.WithArticleResult(AiResultFakers.ArticleResult());

        EvaluateConcertsHandler handler = deps.BuildBatch();

        // Act
        EvaluateConcertsResult result = await handler.Handle(new EvaluateConcertsCommand(), CancellationToken.None);

        // Assert
        result.Failed.Should().Be(1);
        result.Evaluated.Should().Be(1);
        deps.Evaluator.Verify(
            e => e.EvaluateAsync(It.IsAny<ConcertEvaluationInput>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_WithConcertIds_LoadsSpecificConcertsInsteadOfBatchQuery()
    {
        // Arrange
        var venue = DomainEntityFakers.Venue();
        var artist = DomainEntityFakers.Artist("The Headliner");
        var concert = DomainEntityFakers.Concert(venueId: venue.Id);
        concert.UpdateSchedule(new LocalDate(2026, 7, 1), null, null, "America/Chicago");

        EvaluateDependenciesBuilder deps = new();
        deps.WithConcertAndArtist(concert, artist, venue);
        deps.WithEvaluatorResult(AiResultFakers.EvaluationResult(artistNames: ["The Headliner"]));
        deps.WithArticleResult(AiResultFakers.ArticleResult());
        deps.ConcertRepo
            .Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([concert]);

        EvaluateConcertsHandler handler = deps.BuildBatch();

        // Act
        EvaluateConcertsResult result = await handler.Handle(
            new EvaluateConcertsCommand([concert.Id], 20),
            CancellationToken.None);

        // Assert
        result.Evaluated.Should().Be(1);
        deps.ConcertRepo.Verify(
            r => r.GetUnevaluatedAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
        deps.ConcertRepo.Verify(
            r => r.GetByIdsAsync(It.Is<IEnumerable<string>>(ids => ids.Contains(concert.Id)), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithConcertIds_SkipsCancelledConcerts()
    {
        // Arrange
        var venue = DomainEntityFakers.Venue();
        var concert = DomainEntityFakers.Concert(venueId: venue.Id);
        concert.UpdateSchedule(new LocalDate(2026, 7, 1), null, null, "America/Chicago");
        concert.UpdateStatus(ConcertStatus.Cancelled);

        EvaluateDependenciesBuilder deps = new();
        deps.ConcertRepo
            .Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([concert]);

        EvaluateConcertsHandler handler = deps.BuildBatch();

        // Act
        EvaluateConcertsResult result = await handler.Handle(
            new EvaluateConcertsCommand([concert.Id]),
            CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(new EvaluateConcertsResult(0, 0, 0, 0, 0));
        deps.Evaluator.Verify(
            e => e.EvaluateAsync(It.IsAny<ConcertEvaluationInput>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
