namespace TxConcert.Domain.Features.Artists;

public interface ISpotifyService
{
    /// <summary>
    /// Returns the highest-resolution Spotify artist image URL for the given artist ID or Spotify URL,
    /// or null if the artist is not found or the call fails.
    /// </summary>
    Task<string?> GetArtistImageUrlAsync(string spotifyArtistIdOrUrl, CancellationToken ct = default);
}
