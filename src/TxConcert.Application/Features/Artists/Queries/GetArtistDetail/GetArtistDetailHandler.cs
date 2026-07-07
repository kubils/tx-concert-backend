using MediatR;
using NodaTime;
using TxConcert.Domain.Features.Artists;
using TxConcert.Domain.Features.Concerts;
using TxConcert.Domain.Features.Genres;
using TxConcert.Domain.Features.Venues;

namespace TxConcert.Application.Features.Artists.Queries.GetArtistDetail;

public sealed class GetArtistDetailHandler(
    IArtistRepository artistRepository,
    IArtistVibeProfileRepository artistVibeProfileRepository,
    IArtistGenreRepository artistGenreRepository,
    IGenreRepository genreRepository,
    IConcertArtistRepository concertArtistRepository,
    IConcertRepository concertRepository,
    IVenueRepository venueRepository,
    IClock clock)
    : IRequestHandler<GetArtistDetailQuery, ArtistDetailResponse?>
{
    public async Task<ArtistDetailResponse?> Handle(
        GetArtistDetailQuery request,
        CancellationToken ct)
    {
        ArtistEntity? artist = await artistRepository.GetByIdAsync(request.Id, ct);
        if (artist is null) return null;

        // Vibes
        ArtistVibeProfileEntity? vibe =
            await artistVibeProfileRepository.GetByArtistIdAsync(artist.Id, ct);
        ArtistVibesDetailResponse? vibesResponse = vibe is not null
            ? new ArtistVibesDetailResponse(
                vibe.Visuals, vibe.Sound, vibe.Energy,
                vibe.FanInteraction, vibe.TexasSpirit, vibe.AiSummary, vibe.AiModel)
            : null;

        // Genres
        IReadOnlyList<ArtistGenreEntity> artistGenres =
            await artistGenreRepository.GetByArtistIdAsync(artist.Id, ct);

        var genreResponses = new List<ArtistGenreResponse>();
        foreach (ArtistGenreEntity ag in artistGenres)
        {
            GenreEntity? genre = await genreRepository.GetByIdAsync(ag.GenreId, ct);
            if (genre is not null)
                genreResponses.Add(new ArtistGenreResponse(genre.Id, genre.Name, ag.IsPrimary));
        }

        // Upcoming concerts
        IReadOnlyList<ConcertArtistEntity> concertArtists =
            await concertArtistRepository.GetByArtistIdAsync(artist.Id, ct);

        LocalDate today = clock.GetCurrentInstant()
            .InZone(NodaTime.DateTimeZoneProviders.Tzdb["America/Chicago"])
            .Date;

        var concertResponses = new List<ArtistConcertResponse>();
        foreach (ConcertArtistEntity ca in concertArtists)
        {
            ConcertEntity? concert = await concertRepository.GetByIdAsync(ca.ConcertId, ct);
            if (concert is null || concert.EventDate < today) continue;

            VenueEntity? venue = await venueRepository.GetByIdAsync(concert.VenueId, ct);

            concertResponses.Add(new ArtistConcertResponse(
                concert.Id, concert.Name, concert.Slug,
                concert.EventDate, concert.StartTime,
                venue?.Name, venue?.City,
                ca.IsHeadliner));
        }

        // Sort by date
        concertResponses.Sort((a, b) => a.EventDate.CompareTo(b.EventDate));

        return new ArtistDetailResponse(
            artist.Id, artist.Name, artist.Slug,
            artist.Bio, artist.ImageUrl,
            artist.OriginCity, artist.OriginCountry,
            artist.PopularityScore, artist.IsActive,
            artist.SpotifyUrl, artist.InstagramUrl, artist.WebsiteUrl,
            vibesResponse,
            genreResponses,
            concertResponses,
            artist.CreatedAt,
            artist.UpdatedAt);
    }
}
