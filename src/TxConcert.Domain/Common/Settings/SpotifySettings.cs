namespace TxConcert.Domain.Common.Settings;

public sealed class SpotifySettings
{
    public const string SectionName = "Spotify";

    public string ClientId { get; init; } = "";
    public string ClientSecret { get; init; } = "";
    public string TokenEndpoint { get; init; } = "https://accounts.spotify.com/api/token";
    public string ApiBaseUrl { get; init; } = "https://api.spotify.com/v1";
}
