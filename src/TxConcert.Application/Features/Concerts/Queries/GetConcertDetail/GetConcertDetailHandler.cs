using MediatR;
using NodaTime;
using TxConcert.Domain.Features.Artists;
using TxConcert.Domain.Features.Concerts;
using TxConcert.Domain.Features.Venues;

namespace TxConcert.Application.Features.Concerts.Queries.GetConcertDetail;

public sealed class GetConcertDetailHandler(
    IConcertRepository concertRepository,
    IVenueRepository venueRepository,
    IConcertArtistRepository concertArtistRepository,
    IArtistRepository artistRepository,
    IConcertDescriptionRepository concertDescriptionRepository,
    IConcertArticleRepository concertArticleRepository,
    IConcertVibeRepository concertVibeRepository,
    IArtistVibeProfileRepository artistVibeProfileRepository)
    : IRequestHandler<GetConcertDetailQuery, ConcertDetailResponse?>
{
    public async Task<ConcertDetailResponse?> Handle(
        GetConcertDetailQuery request,
        CancellationToken ct)
    {
        ConcertEntity? concert = await concertRepository.GetByIdAsync(request.Id, ct);
        if (concert is null) return null;

        // Venue
        VenueEntity? venue = await venueRepository.GetByIdAsync(concert.VenueId, ct);
        ConcertVenueResponse? venueResponse = venue is not null
            ? new ConcertVenueResponse(
                venue.Id, venue.Name, venue.Slug, venue.City,
                venue.Address, venue.Latitude, venue.Longitude)
            : null;

        // Description (active)
        ConcertDescriptionEntity? description =
            await concertDescriptionRepository.GetActiveForConcertAsync(concert.Id, ct);

        // Article (active)
        ConcertArticleEntity? article =
            await concertArticleRepository.GetActiveForConcertAsync(concert.Id, ct);

        ConcertArticleResponse? articleResponse = article is not null
            ? new ConcertArticleResponse(
                article.Id,
                article.Title,
                article.Spot,
                article.Body,
                article.SeoKeywords,
                article.MetaDescription,
                article.ImageUrl,
                article.ImageAltText,
                article.ImageCredit,
                article.ImageSource,
                article.VersionNumber,
                article.GeneratedAt)
            : null;

        // Vibes
        ConcertVibeEntity? vibe = await concertVibeRepository.GetByConcertIdAsync(concert.Id, ct);
        ConcertVibesResponse? vibesResponse = vibe is not null
            ? new ConcertVibesResponse(
                vibe.Energy, vibe.SoundQuality, vibe.Hype,
                vibe.CrowdVibe, vibe.ValueForMoney, vibe.AiSummary, vibe.AiModel)
            : null;

        // Artists
        IReadOnlyList<ConcertArtistEntity> concertArtists =
            await concertArtistRepository.GetByConcertIdAsync(concert.Id, ct);

        var artistResponses = new List<ConcertArtistResponse>();
        foreach (ConcertArtistEntity ca in concertArtists.OrderBy(ca => ca.BillingOrder))
        {
            ArtistEntity? artist = await artistRepository.GetByIdAsync(ca.ArtistId, ct);
            if (artist is null) continue;

            ArtistVibeProfileEntity? artistVibe =
                await artistVibeProfileRepository.GetByArtistIdAsync(artist.Id, ct);

            artistResponses.Add(new ConcertArtistResponse(
                artist.Id, artist.Name, artist.Slug, artist.ImageUrl,
                ca.IsHeadliner, ca.BillingOrder,
                artistVibe is not null
                    ? new ArtistVibesSummaryResponse(
                        artistVibe.Energy, artistVibe.Sound, artistVibe.TexasSpirit, artistVibe.AiSummary)
                    : null));
        }

        return new ConcertDetailResponse(
            concert.Id, concert.Name, concert.Slug,
            concert.EventDate, concert.DoorsOpen, concert.StartTime, DateTimeZoneProviders.Tzdb[concert.TimeZone],
            concert.ConcertType, concert.Status,
            concert.PriceMin, concert.PriceMax,
            concert.IsSoldOut, concert.IsFeatured,
            concert.TicketUrl,
            concert.ExternalData?.ImageUrl,
            concert.ExternalData?.Info,
            concert.ExternalData?.PleaseNote,
            venueResponse,
            description?.Content,
            articleResponse,
            vibesResponse,
            artistResponses,
            concert.CreatedAt,
            concert.UpdatedAt);
    }
}
