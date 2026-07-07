using NodaTime;
using TxConcert.Domain.Common.Enums;
using TxConcert.Domain.Features.Concerts;

namespace TxConcert.Application.Features.Concerts.Queries;

public sealed record ConcertListResponse(
    string Id,
    string Name,
    string Slug,
    LocalDate EventDate,
    LocalTime? StartTime,
    DateTimeZone TimeZone,
    ConcertType ConcertType,
    ConcertStatus Status,
    decimal? PriceMin,
    decimal? PriceMax,
    bool IsSoldOut,
    bool IsFeatured,
    string? TicketUrl,
    string? VenueName,
    string? VenueCity,
    string? ImageUrl,
    ConcertVibesSummaryResponse? Vibes)
{
    public static ConcertListResponse From(
        ConcertEntity e,
        string? venueName,
        string? venueCity,
        ConcertVibeEntity? vibe) => new(
        e.Id, e.Name, e.Slug, e.EventDate, e.StartTime, DateTimeZoneProviders.Tzdb[e.TimeZone],
        e.ConcertType, e.Status, e.PriceMin, e.PriceMax,
        e.IsSoldOut, e.IsFeatured, e.TicketUrl,
        venueName, venueCity,
        e.ExternalData?.ImageUrl,
        vibe is not null
            ? new ConcertVibesSummaryResponse(vibe.Energy, vibe.Hype, vibe.CrowdVibe, vibe.AiSummary)
            : null);
}

public sealed record ConcertVibesSummaryResponse(
    int Energy,
    int Hype,
    int CrowdVibe,
    string? Summary);

public sealed record ConcertDetailResponse(
    string Id,
    string Name,
    string Slug,
    LocalDate EventDate,
    LocalTime? DoorsOpen,
    LocalTime? StartTime,
    DateTimeZone TimeZone,
    ConcertType ConcertType,
    ConcertStatus Status,
    decimal? PriceMin,
    decimal? PriceMax,
    bool IsSoldOut,
    bool IsFeatured,
    string? TicketUrl,
    string? ImageUrl,
    string? Info,
    string? PleaseNote,
    ConcertVenueResponse? Venue,
    string? Description,
    ConcertArticleResponse? Article,
    ConcertVibesResponse? Vibes,
    IReadOnlyList<ConcertArtistResponse> Artists,
    Instant CreatedAt,
    Instant UpdatedAt);

public sealed record ConcertArticleResponse(
    string Id,
    string Title,
    string Spot,
    string Body,
    IReadOnlyList<string> SeoKeywords,
    string MetaDescription,
    string? ImageUrl,
    string? ImageAltText,
    string? ImageCredit,
    ArticleImageSource ImageSource,
    int VersionNumber,
    Instant GeneratedAt);

public sealed record ConcertVenueResponse(
    string Id,
    string Name,
    string Slug,
    string? City,
    string? Address,
    decimal? Latitude,
    decimal? Longitude);

public sealed record ConcertVibesResponse(
    int Energy,
    int SoundQuality,
    int Hype,
    int CrowdVibe,
    int ValueForMoney,
    string? Summary,
    string? AiModel);

public sealed record ConcertArtistResponse(
    string Id,
    string Name,
    string Slug,
    string? ImageUrl,
    bool IsHeadliner,
    int BillingOrder,
    ArtistVibesSummaryResponse? Vibes);

public sealed record ArtistVibesSummaryResponse(
    int Energy,
    int Sound,
    int TexasSpirit,
    string? Summary);
