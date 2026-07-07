using Bogus;
using NodaTime;
using TxConcert.Domain.Common.Enums;
using TxConcert.Domain.Features.Artists;
using TxConcert.Domain.Features.Concerts;
using TxConcert.Domain.Features.Genres;
using TxConcert.Domain.Features.States;
using TxConcert.Domain.Features.Venues;

namespace TxConcert.UnitTests.TestSupport.Fakers;

public static class DomainEntityFakers
{
    private static readonly Faker _f = new();

    public static VenueEntity Venue(string? name = null, string? stateId = null) =>
        VenueEntity.Create(
            name ?? _f.Company.CompanyName() + " Hall",
            (name ?? _f.Company.CompanyName()).ToLowerInvariant().Replace(' ', '-'),
            _f.Address.StreetAddress(),
            _f.Address.City(),
            stateId ?? "state_" + _f.Random.AlphaNumeric(10),
            "live-music");

    public static ArtistEntity Artist(string? name = null, string? bio = null, string? spotifyUrl = null)
    {
        string n = name ?? _f.Name.FullName();
        var artist = ArtistEntity.Create(n, n.ToLowerInvariant().Replace(' ', '-'), bio ?? "Bio text");
        artist.SetSocialLinks(spotifyUrl, null, null);
        return artist;
    }

    public static StateEntity State(string code = "TX") =>
        StateEntity.Create(code, code);

    public static GenreEntity Genre(string name = "Rock") =>
        GenreEntity.Create(name, name.ToLowerInvariant());

    public static ConcertEntity Concert(string? venueId = null, string? dataHash = null)
    {
        var concert = ConcertEntity.Create(
            _f.Music.Genre() + " Tour",
            _f.Lorem.Slug(),
            venueId ?? DomainEntityFakers.Venue().Id,
            new LocalDate(2026, _f.Random.Int(1, 12), _f.Random.Int(1, 28)),
            ConcertType.Solo);
        if (dataHash is not null)
            concert.UpdateDataHash(dataHash);
        return concert;
    }

    public static ConcertArtistEntity ConcertArtist(string concertId, string artistId, bool isHeadliner = true, int billing = 0) =>
        ConcertArtistEntity.Create(concertId, artistId, isHeadliner, billing);

    public static ConcertDescriptionEntity ActiveDescription(string concertId)
    {
        var d = ConcertDescriptionEntity.Create(
            concertId,
            "Old description",
            "claude-sonnet-4",
            "concert-evaluation-v1",
            1,
            SystemClock.Instance.GetCurrentInstant());
        d.Activate();
        return d;
    }
}
