using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TxConcert.Domain.Common.Settings;
using TxConcert.Domain.Features.Artists;
using TxConcert.Infrastructure.Services.Models;

namespace TxConcert.Infrastructure.Services;

public sealed class SpotifyService(
    HttpClient httpClient,
    IOptions<SpotifySettings> options,
    ILogger<SpotifyService> logger) : ISpotifyService
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private static readonly Regex _artistIdRegex = new(
        @"(?:^|/)artist/([A-Za-z0-9]+)(?:[/?#]|$)",
        RegexOptions.Compiled);

    private string? _cachedToken;
    private DateTimeOffset _cachedTokenExpiresAt = DateTimeOffset.MinValue;
    private readonly SemaphoreSlim _tokenLock = new(1, 1);

    public async Task<string?> GetArtistImageUrlAsync(string spotifyArtistIdOrUrl, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(spotifyArtistIdOrUrl))
            return null;

        string? artistId = ExtractArtistId(spotifyArtistIdOrUrl);
        if (artistId is null)
        {
            logger.LogWarning("Could not extract Spotify artist ID from {Input}", spotifyArtistIdOrUrl);
            return null;
        }

        SpotifySettings settings = options.Value;
        if (string.IsNullOrWhiteSpace(settings.ClientId) || string.IsNullOrWhiteSpace(settings.ClientSecret))
        {
            logger.LogWarning("Spotify credentials not configured — skipping artist image lookup");
            return null;
        }

        try
        {
            string token = await GetAccessTokenAsync(settings, ct);

            using var request = new HttpRequestMessage(HttpMethod.Get, $"{settings.ApiBaseUrl}/artists/{artistId}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            HttpResponseMessage response = await httpClient.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "Spotify artist lookup failed for {ArtistId}: {StatusCode}",
                    artistId, response.StatusCode);
                return null;
            }

            string body = await response.Content.ReadAsStringAsync(ct);
            SpotifyArtistResponse? artist = JsonSerializer.Deserialize<SpotifyArtistResponse>(body, _jsonOptions);

            // Spotify returns images sorted largest-first.
            return artist?.Images?.FirstOrDefault()?.Url;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Spotify image lookup failed for {Input}", spotifyArtistIdOrUrl);
            return null;
        }
    }

    private async Task<string> GetAccessTokenAsync(SpotifySettings settings, CancellationToken ct)
    {
        if (_cachedToken is not null && DateTimeOffset.UtcNow < _cachedTokenExpiresAt)
            return _cachedToken;

        await _tokenLock.WaitAsync(ct);
        try
        {
            if (_cachedToken is not null && DateTimeOffset.UtcNow < _cachedTokenExpiresAt)
                return _cachedToken;

            string basic = Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"{settings.ClientId}:{settings.ClientSecret}"));

            using var tokenRequest = new HttpRequestMessage(HttpMethod.Post, settings.TokenEndpoint);
            tokenRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", basic);
            tokenRequest.Content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials")
            });

            HttpResponseMessage response = await httpClient.SendAsync(tokenRequest, ct);
            response.EnsureSuccessStatusCode();

            string body = await response.Content.ReadAsStringAsync(ct);
            SpotifyTokenResponse? tokenResponse = JsonSerializer.Deserialize<SpotifyTokenResponse>(body, _jsonOptions);

            if (tokenResponse?.AccessToken is null)
                throw new InvalidOperationException("Spotify token response did not contain an access_token");

            _cachedToken = tokenResponse.AccessToken;
            // Refresh 60s before expiry to avoid edge races.
            _cachedTokenExpiresAt = DateTimeOffset.UtcNow.AddSeconds(Math.Max(60, tokenResponse.ExpiresIn - 60));
            return _cachedToken;
        }
        finally
        {
            _tokenLock.Release();
        }
    }

    private static string? ExtractArtistId(string input)
    {
        string trimmed = input.Trim();

        Match match = _artistIdRegex.Match(trimmed);
        if (match.Success)
            return match.Groups[1].Value;

        // Treat raw 22-char base62 IDs as-is.
        if (trimmed.Length is >= 10 and <= 32 && trimmed.All(c => char.IsLetterOrDigit(c)))
            return trimmed;

        return null;
    }
}
