using System.Text.Json.Serialization;

namespace TxConcert.Infrastructure.Services.Models;

internal sealed class SpotifyTokenResponse
{
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }

    [JsonPropertyName("token_type")]
    public string? TokenType { get; set; }

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
}

internal sealed class SpotifyArtistResponse
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public List<SpotifyImage>? Images { get; set; }
}

internal sealed class SpotifyImage
{
    public string? Url { get; set; }
    public int? Height { get; set; }
    public int? Width { get; set; }
}
