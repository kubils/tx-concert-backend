using TxConcert.Domain.Common;
using TxConcert.Domain.Common.Base;

namespace TxConcert.Domain.Features.Artists;

public sealed class ArtistEntity : BaseEntity
{
    private ArtistEntity() { } // EF Core

    public static ArtistEntity Create(
        string name,
        string slug,
        string? bio = null,
        string? imageUrl = null,
        string? originCity = null,
        string? originStateId = null,
        string originCountry = "US")
    {
        var entity = new ArtistEntity
        {
            Name = name,
            Slug = slug,
            Bio = bio,
            ImageUrl = imageUrl,
            OriginCity = originCity,
            OriginStateId = originStateId,
            OriginCountry = originCountry,
            PopularityScore = 0,
            IsActive = true
        };
        entity.SetId(Constants.IdPrefix.Artist);
        return entity;
    }

    public string Name { get; private set; } = default!;
    public string Slug { get; private set; } = default!;
    public string? Bio { get; private set; }
    public string? ImageUrl { get; private set; }
    public string? OriginCity { get; private set; }
    public string? OriginStateId { get; private set; }
    public string OriginCountry { get; private set; } = "US";
    public int PopularityScore { get; private set; }
    public string? SpotifyUrl { get; private set; }
    public string? InstagramUrl { get; private set; }
    public string? WebsiteUrl { get; private set; }
    public bool IsActive { get; private set; } = true;

    public void UpdateProfile(
        string name,
        string slug,
        string? bio,
        string? imageUrl,
        string? originCity,
        string? originStateId,
        string originCountry)
    {
        Name = name;
        Slug = slug;
        Bio = bio;
        ImageUrl = imageUrl;
        OriginCity = originCity;
        OriginStateId = originStateId;
        OriginCountry = originCountry;
    }

    public void SetSocialLinks(string? spotifyUrl, string? instagramUrl, string? websiteUrl)
    {
        SpotifyUrl = spotifyUrl;
        InstagramUrl = instagramUrl;
        WebsiteUrl = websiteUrl;
    }

    public void SetPopularityScore(int score) => PopularityScore = score;
    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
