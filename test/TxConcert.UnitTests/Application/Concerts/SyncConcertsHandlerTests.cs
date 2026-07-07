using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TxConcert.Application.Features.Concerts.Commands.SyncConcerts;
using TxConcert.Domain.Features.Artists;
using TxConcert.Domain.Features.Concerts;
using TxConcert.Domain.Features.Genres;
using TxConcert.Domain.Features.States;
using TxConcert.Domain.Features.Venues;
using TxConcert.UnitTests.TestSupport.Fakers;

namespace TxConcert.UnitTests.Application.Concerts;

public class SyncConcertsHandlerTests
{
    private readonly Mock<ITicketmasterService> _ticketmaster = new();
    private readonly Mock<IConcertRepository> _concertRepo = new();
    private readonly Mock<IVenueRepository> _venueRepo = new();
    private readonly Mock<IArtistRepository> _artistRepo = new();
    private readonly Mock<IGenreRepository> _genreRepo = new();
    private readonly Mock<IStateRepository> _stateRepo = new();
    private readonly Mock<IConcertArtistRepository> _concertArtistRepo = new();
    private readonly Mock<IArtistGenreRepository> _artistGenreRepo = new();
    private readonly Mock<IConcertDescriptionRepository> _descriptionRepo = new();

    private SyncConcertsHandler BuildHandler() => new(
        _ticketmaster.Object,
        _concertRepo.Object,
        _venueRepo.Object,
        _artistRepo.Object,
        _genreRepo.Object,
        _stateRepo.Object,
        _concertArtistRepo.Object,
        _artistGenreRepo.Object,
        _descriptionRepo.Object,
        NullLoggerFactory.Instance.CreateLogger<SyncConcertsHandler>());

    [Fact]
    public async Task Handle_NewTexasEvent_CreatesConcertVenueArtistAndGenre()
    {
        // Arrange
        TicketmasterEventDto ev = new TicketmasterEventDtoFaker()
            .InState("TX")
            .WithTicketmasterId("tm_newevent")
            .WithAttractions("The Headliner")
            .Generate();

        _ticketmaster.Setup(t => t.GetUpcomingEventsAsync(It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([ev]);

        _stateRepo.Setup(r => r.GetByAbbreviationAsync("TX", It.IsAny<CancellationToken>()))
            .ReturnsAsync((StateEntity?)null);
        _venueRepo.Setup(r => r.GetBySlugAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((VenueEntity?)null);
        _genreRepo.Setup(r => r.GetBySlugAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GenreEntity?)null);
        _concertRepo.Setup(r => r.GetByTicketmasterIdAsync("tm_newevent", It.IsAny<CancellationToken>()))
            .ReturnsAsync((ConcertEntity?)null);
        _concertRepo.Setup(r => r.GetBySlugAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ConcertEntity?)null);
        _artistRepo.Setup(r => r.GetBySlugAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ArtistEntity?)null);
        _artistGenreRepo.Setup(r => r.GetByArtistAndGenreAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ArtistGenreEntity?)null);

        SyncConcertsHandler handler = BuildHandler();

        // Act
        SyncConcertsResult result = await handler.Handle(new SyncConcertsCommand(), CancellationToken.None);

        // Assert
        result.Created.Should().Be(1);
        result.Updated.Should().Be(0);
        result.Skipped.Should().Be(0);

        _concertRepo.Verify(r => r.UpsertAsync(It.IsAny<ConcertEntity>(), It.IsAny<CancellationToken>()), Times.Once);
        _venueRepo.Verify(r => r.UpsertAsync(It.IsAny<VenueEntity>(), It.IsAny<CancellationToken>()), Times.Once);
        _artistRepo.Verify(r => r.UpsertAsync(It.IsAny<ArtistEntity>(), It.IsAny<CancellationToken>()), Times.Once);
        _concertArtistRepo.Verify(r => r.UpsertAsync(It.IsAny<ConcertArtistEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ExistingConcertWithChangedData_UpdatesConcertAndDeactivatesOldDescription()
    {
        // Arrange
        TicketmasterEventDto ev = new TicketmasterEventDtoFaker()
            .InState("TX")
            .WithTicketmasterId("tm_existing")
            .Generate();

        StateEntity state = DomainEntityFakers.State();
        VenueEntity venue = DomainEntityFakers.Venue(ev.VenueName!, state.Id);
        ConcertEntity existing = DomainEntityFakers.Concert(venue.Id, dataHash: "old_hash");
        ConcertDescriptionEntity oldDescription = DomainEntityFakers.ActiveDescription(existing.Id);

        _ticketmaster.Setup(t => t.GetUpcomingEventsAsync(It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([ev]);
        _stateRepo.Setup(r => r.GetByAbbreviationAsync("TX", It.IsAny<CancellationToken>()))
            .ReturnsAsync(state);
        _venueRepo.Setup(r => r.GetBySlugAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(venue);
        _genreRepo.Setup(r => r.GetBySlugAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(DomainEntityFakers.Genre());
        _concertRepo.Setup(r => r.GetByTicketmasterIdAsync("tm_existing", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        _descriptionRepo.Setup(r => r.GetActiveForConcertAsync(existing.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(oldDescription);

        SyncConcertsHandler handler = BuildHandler();

        // Act
        SyncConcertsResult result = await handler.Handle(new SyncConcertsCommand(), CancellationToken.None);

        // Assert
        result.Updated.Should().Be(1);
        result.Created.Should().Be(0);

        oldDescription.IsActive.Should().BeFalse("data hash changed so old description must be deactivated for re-evaluation");
        _descriptionRepo.Verify(r => r.UpsertAsync(oldDescription, It.IsAny<CancellationToken>()), Times.Once);
        _concertRepo.Verify(r => r.UpsertAsync(existing, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ExistingConcertWithSameData_DoesNotDeactivateDescription()
    {
        // Arrange
        TicketmasterEventDto ev = new TicketmasterEventDtoFaker()
            .InState("TX")
            .WithTicketmasterId("tm_same")
            .Generate();

        // Compute the exact hash the handler will compute and seed it on the existing concert
        string sameHash = ComputeEventHash(ev);
        StateEntity state = DomainEntityFakers.State();
        VenueEntity venue = DomainEntityFakers.Venue(ev.VenueName!, state.Id);
        ConcertEntity existing = DomainEntityFakers.Concert(venue.Id, dataHash: sameHash);
        ConcertDescriptionEntity oldDescription = DomainEntityFakers.ActiveDescription(existing.Id);

        _ticketmaster.Setup(t => t.GetUpcomingEventsAsync(It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([ev]);
        _stateRepo.Setup(r => r.GetByAbbreviationAsync("TX", It.IsAny<CancellationToken>()))
            .ReturnsAsync(state);
        _venueRepo.Setup(r => r.GetBySlugAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(venue);
        _genreRepo.Setup(r => r.GetBySlugAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(DomainEntityFakers.Genre());
        _concertRepo.Setup(r => r.GetByTicketmasterIdAsync("tm_same", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        SyncConcertsHandler handler = BuildHandler();

        // Act
        await handler.Handle(new SyncConcertsCommand(), CancellationToken.None);

        // Assert — when data hash unchanged, description should not be deactivated
        oldDescription.IsActive.Should().BeTrue();
        _descriptionRepo.Verify(
            r => r.GetActiveForConcertAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_NonTexasEvent_IsSkipped()
    {
        // Arrange
        TicketmasterEventDto ev = new TicketmasterEventDtoFaker()
            .InState("CA")
            .Generate();

        _ticketmaster.Setup(t => t.GetUpcomingEventsAsync(It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([ev]);

        SyncConcertsHandler handler = BuildHandler();

        // Act
        SyncConcertsResult result = await handler.Handle(new SyncConcertsCommand(), CancellationToken.None);

        // Assert
        result.Skipped.Should().Be(1);
        result.Created.Should().Be(0);
        _concertRepo.Verify(r => r.UpsertAsync(It.IsAny<ConcertEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_TicketmasterReturnsEmpty_CompletesWithZeroCounts()
    {
        // Arrange
        _ticketmaster.Setup(t => t.GetUpcomingEventsAsync(It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        SyncConcertsHandler handler = BuildHandler();

        // Act
        SyncConcertsResult result = await handler.Handle(new SyncConcertsCommand(), CancellationToken.None);

        // Assert
        result.Created.Should().Be(0);
        result.Updated.Should().Be(0);
        result.Skipped.Should().Be(0);
        result.EvaluationConcertIds.Should().NotBeNull();
        result.EvaluationConcertIds.Should().BeEmpty();
        _concertRepo.Verify(r => r.UpsertAsync(It.IsAny<ConcertEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_TwoEventsAtSameVenue_ResolvesVenueOnceViaCache()
    {
        // Arrange
        var faker = new TicketmasterEventDtoFaker().InState("TX");
        TicketmasterEventDto ev1 = faker.WithTicketmasterId("tm_a").Generate();
        TicketmasterEventDto ev2 = faker.WithTicketmasterId("tm_b").Generate() with
        {
            VenueName = ev1.VenueName,
            VenueCity = ev1.VenueCity
        };

        _ticketmaster.Setup(t => t.GetUpcomingEventsAsync(It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([ev1, ev2]);

        StateEntity state = DomainEntityFakers.State();
        VenueEntity venue = DomainEntityFakers.Venue(ev1.VenueName!, state.Id);

        _stateRepo.Setup(r => r.GetByAbbreviationAsync("TX", It.IsAny<CancellationToken>()))
            .ReturnsAsync(state);
        _venueRepo.Setup(r => r.GetBySlugAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(venue);
        _concertRepo.Setup(r => r.GetByTicketmasterIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ConcertEntity?)null);
        _concertRepo.Setup(r => r.GetBySlugAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ConcertEntity?)null);
        _genreRepo.Setup(r => r.GetBySlugAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(DomainEntityFakers.Genre());
        _artistRepo.Setup(r => r.GetBySlugAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ArtistEntity?)null);
        _artistGenreRepo.Setup(r => r.GetByArtistAndGenreAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ArtistGenreEntity?)null);

        SyncConcertsHandler handler = BuildHandler();

        // Act
        await handler.Handle(new SyncConcertsCommand(), CancellationToken.None);

        // Assert — second event hit the in-memory cache, so the repository lookup fired only once
        _venueRepo.Verify(
            r => r.GetBySlugAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>Replicates the hash computed in SyncConcertsHandler.ComputeEventHash.</summary>
    private static string ComputeEventHash(TicketmasterEventDto ev)
    {
        var sb = new System.Text.StringBuilder();
        sb.Append(ev.Name);
        sb.Append('|').Append(ev.EventDate);
        sb.Append('|').Append(ev.StartTime);
        sb.Append('|').Append(ev.VenueName);
        sb.Append('|').Append(ev.SalesStatus);
        sb.Append('|').Append(ev.PriceMin);
        sb.Append('|').Append(ev.PriceMax);
        sb.Append('|').Append(ev.Url);
        sb.Append('|').Append(ev.ImageUrl);
        sb.Append('|').Append(ev.Info);
        sb.Append('|').Append(ev.PleaseNote);
        sb.Append('|').Append(ev.GenreName);
        sb.Append('|').Append(ev.SubGenreName);
        sb.Append('|').Append(ev.SegmentName);
        foreach (TicketmasterAttractionDto a in ev.Attractions)
            sb.Append('|').Append(a.Name);

        byte[] hash = System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(sb.ToString()));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    [Fact]
    public async Task Handle_MaxEventsProvided_ForwardsLimitToTicketmaster()
    {
        // Arrange
        _ticketmaster.Setup(t => t.GetUpcomingEventsAsync(20, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        SyncConcertsHandler handler = BuildHandler();

        // Act
        await handler.Handle(new SyncConcertsCommand(20), CancellationToken.None);

        // Assert
        _ticketmaster.Verify(
            t => t.GetUpcomingEventsAsync(20, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_NewOrChangedConcerts_ReturnsIdsForEvaluation()
    {
        // Arrange
        TicketmasterEventDto newEvent = new TicketmasterEventDtoFaker()
            .InState("TX")
            .WithTicketmasterId("tm_new_eval")
            .Generate();

        TicketmasterEventDto changedEvent = new TicketmasterEventDtoFaker()
            .InState("TX")
            .WithTicketmasterId("tm_changed_eval")
            .Generate();

        StateEntity state = DomainEntityFakers.State();
        VenueEntity venue = DomainEntityFakers.Venue(newEvent.VenueName!, state.Id);
        ConcertEntity existing = DomainEntityFakers.Concert(venue.Id, dataHash: "old_hash");
        ConcertDescriptionEntity oldDescription = DomainEntityFakers.ActiveDescription(existing.Id);

        _ticketmaster.Setup(t => t.GetUpcomingEventsAsync(It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([newEvent, changedEvent]);
        _stateRepo.Setup(r => r.GetByAbbreviationAsync("TX", It.IsAny<CancellationToken>()))
            .ReturnsAsync(state);
        _venueRepo.Setup(r => r.GetBySlugAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(venue);
        _genreRepo.Setup(r => r.GetBySlugAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(DomainEntityFakers.Genre());
        _concertRepo.Setup(r => r.GetByTicketmasterIdAsync("tm_new_eval", It.IsAny<CancellationToken>()))
            .ReturnsAsync((ConcertEntity?)null);
        _concertRepo.Setup(r => r.GetBySlugAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ConcertEntity?)null);
        _artistRepo.Setup(r => r.GetBySlugAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ArtistEntity?)null);
        _artistGenreRepo.Setup(r => r.GetByArtistAndGenreAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ArtistGenreEntity?)null);
        _concertRepo.Setup(r => r.GetByTicketmasterIdAsync("tm_changed_eval", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        _descriptionRepo.Setup(r => r.GetActiveForConcertAsync(existing.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(oldDescription);

        SyncConcertsHandler handler = BuildHandler();

        // Act
        SyncConcertsResult result = await handler.Handle(new SyncConcertsCommand(), CancellationToken.None);

        // Assert
        result.EvaluationConcertIds.Should().NotBeNull();
        result.EvaluationConcertIds.Should().HaveCount(2);
        result.EvaluationConcertIds.Should().Contain(existing.Id);
    }
}
