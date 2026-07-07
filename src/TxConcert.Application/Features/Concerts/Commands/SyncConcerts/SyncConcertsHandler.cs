using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using MediatR;
using Microsoft.Extensions.Logging;
using TxConcert.Domain.Common.Enums;
using TxConcert.Domain.Features.Artists;
using TxConcert.Domain.Features.Concerts;
using TxConcert.Domain.Features.Genres;
using TxConcert.Domain.Features.States;
using TxConcert.Domain.Features.Venues;

namespace TxConcert.Application.Features.Concerts.Commands.SyncConcerts;

public sealed class SyncConcertsHandler(
    ITicketmasterService ticketmasterService,
    IConcertRepository concertRepository,
    IVenueRepository venueRepository,
    IArtistRepository artistRepository,
    IGenreRepository genreRepository,
    IStateRepository stateRepository,
    IConcertArtistRepository concertArtistRepository,
    IArtistGenreRepository artistGenreRepository,
    IConcertDescriptionRepository concertDescriptionRepository,
    ILogger<SyncConcertsHandler> logger)
    : IRequestHandler<SyncConcertsCommand, SyncConcertsResult>
{
    // In-memory caches to avoid repeated DB lookups within a single sync run
    private readonly Dictionary<string, StateEntity> _stateCache = new();
    private readonly Dictionary<string, VenueEntity> _venueCache = new();
    private readonly Dictionary<string, ArtistEntity> _artistCache = new();
    private readonly Dictionary<string, GenreEntity> _genreCache = new();

    public async Task<SyncConcertsResult> Handle(SyncConcertsCommand request, CancellationToken ct)
    {
        logger.LogInformation("Starting concert sync with max events {MaxEvents}", request.MaxEvents);

        IReadOnlyList<TicketmasterEventDto> events =
            await ticketmasterService.GetUpcomingEventsAsync(request.MaxEvents, ct);

        logger.LogInformation("Received {EventCount} events from Ticketmaster for sync", events.Count);

        int created = 0, updated = 0, skipped = 0;
        List<string> evaluationConcertIds = [];

        foreach (TicketmasterEventDto ev in events)
        {
            try
            {
                // Only process Texas events
                if (ev.VenueStateCode is not "TX")
                {
                    skipped++;
                    continue;
                }

                // 1. Resolve state
                StateEntity state = await ResolveStateAsync(ev.VenueStateCode, ct);

                // 2. Resolve venue
                VenueEntity? venue = null;
                if (ev.VenueName is not null && ev.VenueCity is not null)
                    venue = await ResolveVenueAsync(ev, state.Id, ct);

                if (venue is null)
                {
                    skipped++;
                    continue;
                }

                // 3. Resolve genre
                GenreEntity? genre = null;
                if (ev.GenreName is not null)
                    genre = await ResolveGenreAsync(ev.GenreName, ct);

                // 4. Upsert concert
                string concertSlug = Slugify($"{ev.Name}-{ev.EventDate}");
                ConcertEntity? existing = await concertRepository.GetByTicketmasterIdAsync(ev.TicketmasterId, ct)
                    ?? await concertRepository.GetBySlugAsync(concertSlug, ct);

                string timeZone = ev.TimeZone ?? "America/Chicago";

                var externalData = new ConcertExternalData
                {
                    ImageUrl = ev.ImageUrl,
                    SeatmapUrl = ev.SeatmapUrl,
                    TicketLimitInfo = ev.TicketLimitInfo,
                    SegmentName = ev.SegmentName,
                    SubGenreName = ev.SubGenreName,
                    Info = ev.Info,
                    PleaseNote = ev.PleaseNote
                };

                string dataHash = ComputeEventHash(ev);

                if (existing is not null)
                {
                    existing.UpdateSchedule(ev.EventDate, null, ev.StartTime, timeZone);
                    existing.SetTicketInfo(ev.Url, ev.PriceMin, ev.PriceMax);
                    existing.SetTicketmasterId(ev.TicketmasterId);
                    existing.SetExternalData(externalData);
                    UpdateConcertStatus(existing, ev.SalesStatus);

                    // If data changed, invalidate the AI evaluation so it gets re-evaluated
                    bool dataChanged = existing.UpdateDataHash(dataHash);
                    if (dataChanged)
                    {
                        ConcertDescriptionEntity? activeDesc =
                            await concertDescriptionRepository.GetActiveForConcertAsync(existing.Id, ct);
                        if (activeDesc is not null)
                        {
                            activeDesc.Deactivate();
                            await concertDescriptionRepository.UpsertAsync(activeDesc, ct);
                            logger.LogInformation(
                                "Concert {ConcertId} data changed — marked for re-evaluation",
                                existing.Id);
                        }

                        evaluationConcertIds.Add(existing.Id);
                    }

                    await concertRepository.UpsertAsync(existing, ct);
                    logger.LogDebug("Updated concert {ConcertId} from Ticketmaster event {EventId}", existing.Id, ev.TicketmasterId);
                    updated++;
                }
                else
                {
                    var concert = ConcertEntity.Create(
                        ev.Name,
                        concertSlug,
                        venue.Id,
                        ev.EventDate,
                        ConcertType.Solo,
                        timeZone,
                        startTime: ev.StartTime);

                    concert.SetTicketInfo(ev.Url, ev.PriceMin, ev.PriceMax);
                    concert.SetTicketmasterId(ev.TicketmasterId);
                    concert.SetExternalData(externalData);
                    concert.UpdateDataHash(dataHash);
                    UpdateConcertStatus(concert, ev.SalesStatus);
                    
                    await concertRepository.UpsertAsync(concert, ct);
                    evaluationConcertIds.Add(concert.Id);
                    logger.LogInformation("Created concert {ConcertId} from Ticketmaster event {EventId}", concert.Id, ev.TicketmasterId);

                    // 5. Link artists
                    for (int i = 0; i < ev.Attractions.Count; i++)
                    {
                        TicketmasterAttractionDto attraction = ev.Attractions[i];
                        ArtistEntity artist = await ResolveArtistAsync(attraction, ct);

                        ConcertArtistEntity concertArtist = ConcertArtistEntity.Create(
                            concert.Id,
                            artist.Id,
                            isHeadliner: i == 0,
                            billingOrder: i);

                        await concertArtistRepository.UpsertAsync(concertArtist, ct);

                        // Link artist to genre — prefer the attraction's own genre, fall back to event-level
                        string? artistGenreName = attraction.GenreName ?? ev.GenreName;
                        if (artistGenreName is not null)
                        {
                            GenreEntity artistGenre = await ResolveGenreAsync(artistGenreName, ct);
                            ArtistGenreEntity? existingAg = await artistGenreRepository.GetByArtistAndGenreAsync(
                                artist.Id, artistGenre.Id, ct);

                            if (existingAg is null)
                            {
                                ArtistGenreEntity ag = ArtistGenreEntity.Create(artist.Id, artistGenre.Id, isPrimary: true);
                                await artistGenreRepository.UpsertAsync(ag, ct);
                            }
                        }
                    }

                    created++;
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to sync Ticketmaster event {EventId}: {Name}", ev.TicketmasterId, ev.Name);
                skipped++;
            }
        }

        logger.LogInformation(
            "Sync complete: {Created} created, {Updated} updated, {Skipped} skipped, {QueuedForEvaluation} queued for evaluation",
            created,
            updated,
            skipped,
            evaluationConcertIds.Count);

        return new SyncConcertsResult(created, updated, skipped, evaluationConcertIds);
    }

    private async Task<StateEntity> ResolveStateAsync(string stateCode, CancellationToken ct)
    {
        if (_stateCache.TryGetValue(stateCode, out StateEntity? cached))
            return cached;

        StateEntity? state = await stateRepository.GetByAbbreviationAsync(stateCode, ct);
        if (state is null)
        {
            state = StateEntity.Create(stateCode, stateCode);
            await stateRepository.UpsertAsync(state, ct);
        }

        _stateCache[stateCode] = state;
        return state;
    }

    private async Task<VenueEntity> ResolveVenueAsync(TicketmasterEventDto ev, string stateId, CancellationToken ct)
    {
        string venueSlug = Slugify(ev.VenueName!);
        if (_venueCache.TryGetValue(venueSlug, out VenueEntity? cached))
            return cached;

        VenueEntity? venue = await venueRepository.GetBySlugAsync(venueSlug, ct);
        if (venue is null)
        {
            venue = VenueEntity.Create(
                ev.VenueName!,
                venueSlug,
                ev.VenueAddress ?? "",
                ev.VenueCity!,
                stateId,
                "live-music",
                zipCode: ev.VenueZipCode);

            if (ev.VenueLatitude.HasValue && ev.VenueLongitude.HasValue)
            {
                decimal latitude = ev.VenueLatitude.Value;
                decimal longitude = ev.VenueLongitude.Value;
                if (IsValidCoordinate(latitude) && IsValidCoordinate(longitude))
                {
                    venue.SetCoordinates(latitude, longitude);
                }
                else
                {
                    logger.LogWarning(
                        "Skipping invalid venue coordinates for event {EventId}: lat={Latitude}, lon={Longitude}",
                        ev.TicketmasterId,
                        latitude,
                        longitude);
                }
            }

            await venueRepository.UpsertAsync(venue, ct);
        }

        _venueCache[venueSlug] = venue;
        return venue;
    }

    private static bool IsValidCoordinate(decimal value)
        => value >= -180m && value <= 180m;

    private async Task<GenreEntity> ResolveGenreAsync(string genreName, CancellationToken ct)
    {
        string genreSlug = Slugify(genreName);
        if (_genreCache.TryGetValue(genreSlug, out GenreEntity? cached))
            return cached;

        GenreEntity? genre = await genreRepository.GetBySlugAsync(genreSlug, ct);
        if (genre is null)
        {
            genre = GenreEntity.Create(genreName, genreSlug);
            await genreRepository.UpsertAsync(genre, ct);
        }

        _genreCache[genreSlug] = genre;
        return genre;
    }

    private async Task<ArtistEntity> ResolveArtistAsync(TicketmasterAttractionDto attraction, CancellationToken ct)
    {
        string artistSlug = Slugify(attraction.Name);
        if (_artistCache.TryGetValue(artistSlug, out ArtistEntity? cached))
            return cached;

        ArtistEntity? artist = await artistRepository.GetBySlugAsync(artistSlug, ct);
        if (artist is null)
        {
            artist = ArtistEntity.Create(attraction.Name, artistSlug, imageUrl: attraction.ImageUrl);
            artist.SetSocialLinks(attraction.SpotifyUrl, attraction.InstagramUrl, attraction.WebsiteUrl);
            await artistRepository.UpsertAsync(artist, ct);
        }

        _artistCache[artistSlug] = artist;
        return artist;
    }

    private static void UpdateConcertStatus(ConcertEntity concert, string? salesStatus)
    {
        ConcertStatus status = salesStatus switch
        {
            "onsale" => ConcertStatus.OnSale,
            "offsale" => ConcertStatus.Scheduled,
            "cancelled" => ConcertStatus.Cancelled,
            "postponed" => ConcertStatus.Postponed,
            "rescheduled" => ConcertStatus.Rescheduled,
            _ => ConcertStatus.Scheduled
        };
        concert.UpdateStatus(status);
    }

    private static string ComputeEventHash(TicketmasterEventDto ev)
    {
        var sb = new StringBuilder();
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

        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(sb.ToString()));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static string Slugify(string input)
    {
        string slug = input.ToLowerInvariant().Trim();
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = Regex.Replace(slug, @"[\s]+", "-");
        slug = Regex.Replace(slug, @"-+", "-");
        return slug.Trim('-');
    }
}
