using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TxConcert.Domain.Common.Settings;
using TxConcert.Domain.Features.Concerts;
using TxConcert.Infrastructure.Services.Models;
using TxConcert.Infrastructure.Services.Prompts;

namespace TxConcert.Infrastructure.Services;

public sealed class ClaudeConcertArticleWriter(
    HttpClient httpClient,
    IOptions<ClaudeSettings> options,
    ILogger<ClaudeConcertArticleWriter> logger) : IConcertArticleWriter
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task<ConcertArticleResult> WriteArticleAsync(
        ConcertArticleInput input,
        CancellationToken ct = default)
    {
        ClaudeSettings settings = options.Value;

        string systemPrompt = ConcertArticleWriterPrompts.BuildSystemPrompt();
        string userPrompt = ConcertArticleWriterPrompts.BuildUserPrompt(input);

        logger.LogInformation("Writing article for concert: {ConcertName}", input.ConcertName);

        var requestBody = new
        {
            model = settings.Model,
            max_tokens = settings.MaxTokens,
            temperature = 0.7,
            system = systemPrompt,
            messages = new[]
            {
                new { role = "user", content = userPrompt }
            }
        };

        string json = JsonSerializer.Serialize(requestBody, _jsonOptions);

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.anthropic.com/v1/messages");
        request.Headers.Add("x-api-key", settings.ApiKey);
        request.Headers.Add("anthropic-version", "2023-06-01");
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        var stopwatch = Stopwatch.StartNew();
        HttpResponseMessage response = await httpClient.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        string responseJson = await response.Content.ReadAsStringAsync(ct);
        stopwatch.Stop();

        ClaudeResponse? claudeResponse = JsonSerializer.Deserialize<ClaudeResponse>(responseJson, _jsonOptions);

        string responseText = claudeResponse?.Content?
            .Where(c => c.Type == "text")
            .Select(c => c.Text)
            .FirstOrDefault() ?? "";

        int? tokensUsed = (claudeResponse?.Usage?.InputTokens ?? 0)
            + (claudeResponse?.Usage?.OutputTokens ?? 0);

        logger.LogInformation(
            "Article writing complete for {ConcertName}. Tokens: {InputTokens}/{OutputTokens}",
            input.ConcertName,
            claudeResponse?.Usage?.InputTokens,
            claudeResponse?.Usage?.OutputTokens);

        return ParseResponse(responseText, input, settings.Model, tokensUsed, stopwatch.ElapsedMilliseconds);
    }

    private static ConcertArticleResult ParseResponse(
        string responseText,
        ConcertArticleInput input,
        string aiModel,
        int? tokensUsed,
        long generationTimeMs)
    {
        string json = responseText.Trim();
        if (json.StartsWith("```"))
        {
            int firstNewline = json.IndexOf('\n');
            int lastFence = json.LastIndexOf("```");
            if (firstNewline > 0 && lastFence > firstNewline)
                json = json[(firstNewline + 1)..lastFence].Trim();
        }

        AiArticleResponse? parsed;
        try
        {
            parsed = JsonSerializer.Deserialize<AiArticleResponse>(json, _jsonOptions);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                $"Failed to parse article JSON from Claude response: {ex.Message}", ex);
        }

        if (parsed is null
            || string.IsNullOrWhiteSpace(parsed.Title)
            || string.IsNullOrWhiteSpace(parsed.Body))
        {
            throw new InvalidOperationException(
                "Claude article response did not contain required fields (title, body).");
        }

        ArticleImageSource chosenSource = ParseImageSource(parsed.ChosenImageSource);
        (string? imageUrl, string? imageCredit) = ResolveImage(chosenSource, input);

        // If the model asked for a source but the URL is missing, downgrade to None.
        if (chosenSource != ArticleImageSource.None && imageUrl is null)
        {
            chosenSource = ArticleImageSource.None;
            imageCredit = null;
        }

        return new ConcertArticleResult
        {
            Title = parsed.Title!.Trim(),
            Spot = (parsed.Spot ?? "").Trim(),
            Body = parsed.Body!.Trim(),
            SeoKeywords = parsed.SeoKeywords ?? [],
            MetaDescription = (parsed.MetaDescription ?? "").Trim(),
            ImageUrl = imageUrl,
            ImageAltText = imageUrl is not null ? (parsed.ImageAltText ?? "").Trim() : null,
            ImageCredit = imageCredit,
            ImageSource = chosenSource,
            AiModel = aiModel,
            TokensUsed = tokensUsed,
            GenerationTimeMs = generationTimeMs
        };
    }

    private static int CountParagraphs(string body)
    {
        return body
            .Split(new[] { "\n\n" }, StringSplitOptions.RemoveEmptyEntries)
            .Count(paragraph => !string.IsNullOrWhiteSpace(paragraph));
    }

    private static ArticleImageSource ParseImageSource(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return ArticleImageSource.None;

        return raw.Trim().ToLowerInvariant() switch
        {
            "spotify" => ArticleImageSource.Spotify,
            "ticketmaster" => ArticleImageSource.Ticketmaster,
            "unsplash" => ArticleImageSource.Unsplash,
            "pexels" => ArticleImageSource.Pexels,
            "manual" => ArticleImageSource.Manual,
            _ => ArticleImageSource.None
        };
    }

    private static (string? imageUrl, string? credit) ResolveImage(
        ArticleImageSource source,
        ConcertArticleInput input)
    {
        switch (source)
        {
            case ArticleImageSource.Spotify:
                // Prefer headliner image, fall back to first artist with a Spotify image.
                ArticleArtistInput? headliner = input.Artists.FirstOrDefault(a =>
                    a.IsHeadliner && !string.IsNullOrWhiteSpace(a.SpotifyImageUrl));
                ArticleArtistInput? anyWithSpotify = headliner ?? input.Artists.FirstOrDefault(a =>
                    !string.IsNullOrWhiteSpace(a.SpotifyImageUrl));
                return anyWithSpotify?.SpotifyImageUrl is { } url
                    ? (url, "Photo via Spotify API")
                    : (null, null);

            case ArticleImageSource.Ticketmaster:
                return !string.IsNullOrWhiteSpace(input.TicketmasterImageUrl)
                    ? (input.TicketmasterImageUrl, "Photo via Ticketmaster API")
                    : (null, null);

            default:
                return (null, null);
        }
    }
}
