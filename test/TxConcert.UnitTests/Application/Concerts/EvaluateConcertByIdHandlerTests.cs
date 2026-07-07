using FluentAssertions;
using Moq;
using TxConcert.Application.Features.Concerts.Commands.EvaluateConcertById;
using TxConcert.Domain.Features.Artists;
using TxConcert.Domain.Features.Concerts;
using TxConcert.UnitTests.TestSupport.Builders;
using TxConcert.UnitTests.TestSupport.Fakers;

namespace TxConcert.UnitTests.Application.Concerts;

public class EvaluateConcertByIdHandlerTests
{
    [Fact]
    public async Task Handle_ValidConcert_SavesDescriptionVibeAndArticle()
    {
        // Arrange
        var venue = DomainEntityFakers.Venue();
        var artist = DomainEntityFakers.Artist("The Headliner");
        var concert = DomainEntityFakers.Concert(venueId: venue.Id);

        EvaluateDependenciesBuilder deps = new();
        deps.WithConcertAndArtist(concert, artist, venue);
        deps.WithEvaluatorResult(AiResultFakers.EvaluationResult(artistNames: ["The Headliner"]));
        deps.WithArticleResult(AiResultFakers.ArticleResult(title: "Epic Night"));

        EvaluateConcertByIdHandler handler = deps.BuildById();

        // Act
        EvaluateConcertByIdResult result = await handler.Handle(
            new EvaluateConcertByIdCommand(concert.Id), CancellationToken.None);

        // Assert
        result.ConcertId.Should().Be(concert.Id);
        result.ArticleGenerated.Should().BeTrue();
        result.ArticleTitle.Should().Be("Epic Night");

        deps.DescriptionRepo.Verify(
            r => r.UpsertAsync(It.IsAny<ConcertDescriptionEntity>(), It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
        deps.VibeRepo.Verify(
            r => r.UpsertAsync(It.IsAny<ConcertVibeEntity>(), It.IsAny<CancellationToken>()),
            Times.Once);
        deps.ArtistVibeRepo.Verify(
            r => r.UpsertAsync(It.IsAny<ArtistVibeProfileEntity>(), It.IsAny<CancellationToken>()),
            Times.Once);
        deps.ArticleRepo.Verify(
            r => r.UpsertAsync(It.IsAny<ConcertArticleEntity>(), It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task Handle_EvaluatorReturnsBoundaryScores_StoresThoseScoresOnVibeEntity()
    {
        // Arrange
        var venue = DomainEntityFakers.Venue();
        var artist = DomainEntityFakers.Artist("The Headliner");
        var concert = DomainEntityFakers.Concert(venueId: venue.Id);

        EvaluateDependenciesBuilder deps = new();
        deps.WithConcertAndArtist(concert, artist, venue);
        deps.WithEvaluatorResult(AiResultFakers.EvaluationResult(score: 10, artistNames: ["The Headliner"]));
        deps.WithArticleResult(AiResultFakers.ArticleResult());

        ConcertVibeEntity? savedVibe = null;
        deps.VibeRepo
            .Setup(r => r.UpsertAsync(It.IsAny<ConcertVibeEntity>(), It.IsAny<CancellationToken>()))
            .Callback<ConcertVibeEntity, CancellationToken>((v, _) => savedVibe = v)
            .Returns(Task.CompletedTask);

        EvaluateConcertByIdHandler handler = deps.BuildById();

        // Act
        await handler.Handle(new EvaluateConcertByIdCommand(concert.Id), CancellationToken.None);

        // Assert
        savedVibe.Should().NotBeNull();
        savedVibe!.Energy.Should().Be(10);
        savedVibe.SoundQuality.Should().Be(10);
        savedVibe.Hype.Should().Be(10);
        savedVibe.CrowdVibe.Should().Be(10);
        savedVibe.ValueForMoney.Should().Be(10);
    }

    [Fact]
    public async Task Handle_ArticleWriterThrows_EvaluationStillSucceedsWithErrorReported()
    {
        // Arrange
        var venue = DomainEntityFakers.Venue();
        var artist = DomainEntityFakers.Artist("The Headliner");
        var concert = DomainEntityFakers.Concert(venueId: venue.Id);

        EvaluateDependenciesBuilder deps = new();
        deps.WithConcertAndArtist(concert, artist, venue);
        deps.WithEvaluatorResult(AiResultFakers.EvaluationResult(artistNames: ["The Headliner"]));
        deps.WithArticleWriterThrows(new InvalidOperationException("Claude article error"));

        EvaluateConcertByIdHandler handler = deps.BuildById();

        // Act
        EvaluateConcertByIdResult result = await handler.Handle(
            new EvaluateConcertByIdCommand(concert.Id), CancellationToken.None);

        // Assert
        result.ArticleGenerated.Should().BeFalse();
        result.ArticleError.Should().Contain("Claude article error");
        deps.DescriptionRepo.Verify(
            r => r.UpsertAsync(It.IsAny<ConcertDescriptionEntity>(), It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
        deps.VibeRepo.Verify(
            r => r.UpsertAsync(It.IsAny<ConcertVibeEntity>(), It.IsAny<CancellationToken>()),
            Times.Once);
        deps.ArticleRepo.Verify(
            r => r.UpsertAsync(It.IsAny<ConcertArticleEntity>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ConcertNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        EvaluateDependenciesBuilder deps = new();
        deps.ConcertRepo
            .Setup(r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ConcertEntity?)null);

        EvaluateConcertByIdHandler handler = deps.BuildById();

        // Act
        Func<Task> act = () => handler.Handle(
            new EvaluateConcertByIdCommand("missing_concert_id"), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*missing_concert_id*");
        deps.Evaluator.Verify(
            e => e.EvaluateAsync(It.IsAny<ConcertEvaluationInput>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
