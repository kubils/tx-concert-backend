using NodaTime;
using TxConcert.Domain.Features.Artists;

namespace TxConcert.Application.Features.Artists.Queries;

public sealed record ArtistListResponse(
    string Id,
    string Name,
    string Slug,
    string? ImageUrl,
    int PopularityScore,
    bool IsActive,
    ArtistVibesSummary? Vibes)
{
    public static ArtistListResponse From(ArtistEntity e, ArtistVibeProfileEntity? vibe) => new(
        e.Id, e.Name, e.Slug, e.ImageUrl, e.PopularityScore, e.IsActive,
        vibe is not null
            ? new ArtistVibesSummary(vibe.Energy, vibe.Sound, vibe.TexasSpirit, vibe.AiSummary)
            : null);
}

public sealed record ArtistVibesSummary(
    int Energy,
    int Sound,
    int TexasSpirit,
    string? Summary);

public sealed record ArtistDetailResponse(
    string Id,
    string Name,
    string Slug,
    string? Bio,
    string? ImageUrl,
    string? OriginCity,
    string? OriginCountry,
    int PopularityScore,
    bool IsActive,
    string? SpotifyUrl,
    string? InstagramUrl,
    string? WebsiteUrl,
    ArtistVibesDetailResponse? Vibes,
    IReadOnlyList<ArtistGenreResponse> Genres,
    IReadOnlyList<ArtistConcertResponse> UpcomingConcerts,
    Instant CreatedAt,
    Instant UpdatedAt);

public sealed record ArtistVibesDetailResponse(
    int Visuals,
    int Sound,
    int Energy,
    int FanInteraction,
    int TexasSpirit,
    string? Summary,
    string? AiModel);

public sealed record ArtistGenreResponse(
    string Id,
    string Name,
    bool IsPrimary);

public sealed record ArtistConcertResponse(
    string Id,
    string Name,
    string Slug,
    LocalDate EventDate,
    LocalTime? StartTime,
    string? VenueName,
    string? VenueCity,
    bool IsHeadliner);
