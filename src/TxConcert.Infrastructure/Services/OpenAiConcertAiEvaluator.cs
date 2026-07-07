using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TxConcert.Domain.Common.Settings;
using TxConcert.Domain.Features.Concerts;
using TxConcert.Domain.Prompts;
using TxConcert.Infrastructure.Services.Models;
using TxConcert.Infrastructure.Services.Prompts;

namespace TxConcert.Infrastructure.Services;

public sealed class OpenAiConcertAiEvaluator(
    HttpClient httpClient,
    IBraveSearchService braveSearch,
    IAiPromptProvider promptProvider,
    IOptions<OpenAiSettings> options,
    ILogger<OpenAiConcertAiEvaluator> logger) : IConcertAiEvaluator
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task<ConcertEvaluationResult> EvaluateAsync(
        ConcertEvaluationInput input,
        CancellationToken ct = default)
    {
        OpenAiSettings settings = options.Value;

        // Enrich input with web research before AI call
        await EnrichWithWebResearchAsync(input, ct);

        AiPromptContent prompt = await promptProvider.GetConcertEvaluationPromptAsync(ct);
        string systemPrompt = prompt.SystemPrompt;
        string userPrompt = ConcertEvaluationPrompts.BuildUserPrompt(input, prompt.UserPromptInstructions);

        logger.LogInformation(
            "Evaluating concert with OpenAI: {ConcertName} (with {ResearchCount} web research entries, prompt source {PromptSource}, version {PromptVersion})",
            input.ConcertName,
            input.WebResearch.Count,
            prompt.Source,
            prompt.VersionNumber);

        var requestBody = new
        {
            model = settings.Model,
            max_tokens = settings.MaxTokens,
            temperature = 0.7,
            messages = new object[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userPrompt }
            }
        };

        string json = JsonSerializer.Serialize(requestBody, _jsonOptions);

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
        request.Headers.Add("Authorization", $"Bearer {settings.ApiKey}");
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await httpClient.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        string responseJson = await response.Content.ReadAsStringAsync(ct);
        OpenAiChatResponse? chatResponse = JsonSerializer.Deserialize<OpenAiChatResponse>(responseJson, _jsonOptions);

        string responseText = chatResponse?.Choices?.FirstOrDefault()?.Message?.Content ?? "";

        logger.LogInformation("OpenAI evaluation complete for {ConcertName}. Tokens: {PromptTokens}/{CompletionTokens}",
            input.ConcertName,
            chatResponse?.Usage?.PromptTokens,
            chatResponse?.Usage?.CompletionTokens);

        return ParseResponse(responseText, settings.Model);
    }

    private async Task EnrichWithWebResearchAsync(ConcertEvaluationInput input, CancellationToken ct)
    {
        // Search for each artist — skip artists that already have a vibe profile
        foreach (ArtistEvaluationInput artist in input.Artists)
        {
            if (artist.HasExistingVibeProfile)
            {
                logger.LogDebug("Skipping web research for {ArtistName} — vibe profile already exists", artist.Name);
                continue;
            }

            try
            {
                string query = $"{artist.Name} live concert review Texas";
                WebSearchResult result = await braveSearch.SearchAsync(query, ct);
                string summary = BuildResearchSummary(result);
                if (!string.IsNullOrWhiteSpace(summary))
                    input.WebResearch[$"Artist: {artist.Name}"] = summary;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Brave Search failed for artist: {ArtistName}", artist.Name);
            }
        }

        if (input.VenueName is not null)
        {
            try
            {
                string query = $"{input.VenueName} {input.VenueCity ?? "Texas"} concert venue review";
                WebSearchResult result = await braveSearch.SearchAsync(query, ct);
                string summary = BuildResearchSummary(result);
                if (!string.IsNullOrWhiteSpace(summary))
                    input.WebResearch[$"Venue: {input.VenueName}"] = summary;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Brave Search failed for venue: {VenueName}", input.VenueName);
            }
        }
    }

    private static string BuildResearchSummary(WebSearchResult result)
    {
        if (result.Items.Count == 0) return "";

        var sb = new StringBuilder();
        foreach (WebSearchItem item in result.Items)
        {
            sb.AppendLine($"- {item.Title}: {item.Description}");
            foreach (string snippet in item.ExtraSnippets.Take(2))
            {
                sb.AppendLine($"  {snippet}");
            }
        }

        string summary = sb.ToString();
        return summary.Length > 500 ? summary[..500] + "..." : summary;
    }

    private static ConcertEvaluationResult ParseResponse(string responseText, string aiModel)
    {
        string json = responseText.Trim();
        if (json.StartsWith("```"))
        {
            int firstNewline = json.IndexOf('\n');
            int lastFence = json.LastIndexOf("```");
            if (firstNewline > 0 && lastFence > firstNewline)
                json = json[(firstNewline + 1)..lastFence].Trim();
        }

        AiEvaluationResponse? parsed = JsonSerializer.Deserialize<AiEvaluationResponse>(json, _jsonOptions);

        if (parsed is null)
        {
            return new ConcertEvaluationResult
            {
                Description = responseText,
                AiModel = aiModel,
                ConcertVibes = new ConcertVibeScores
                {
                    Energy = 5, SoundQuality = 5, Hype = 5, CrowdVibe = 5, ValueForMoney = 5,
                    Summary = "AI evaluation could not be fully parsed."
                }
            };
        }

        return new ConcertEvaluationResult
        {
            Description = parsed.Description ?? responseText,
            AiModel = aiModel,
            ConcertVibes = new ConcertVibeScores
            {
                Energy = Clamp(parsed.ConcertVibes?.Energy ?? 5),
                SoundQuality = Clamp(parsed.ConcertVibes?.SoundQuality ?? 5),
                Hype = Clamp(parsed.ConcertVibes?.Hype ?? 5),
                CrowdVibe = Clamp(parsed.ConcertVibes?.CrowdVibe ?? 5),
                ValueForMoney = Clamp(parsed.ConcertVibes?.ValueForMoney ?? 5),
                Summary = parsed.ConcertVibes?.Summary
            },
            ArtistVibes = parsed.ArtistVibes?.Select(a => new ArtistVibeResult
            {
                ArtistName = a.ArtistName ?? "",
                Visuals = Clamp(a.Visuals),
                Sound = Clamp(a.Sound),
                Energy = Clamp(a.Energy),
                FanInteraction = Clamp(a.FanInteraction),
                TexasSpirit = Clamp(a.TexasSpirit),
                Summary = a.Summary
            }).ToList() ?? []
        };
    }

    private static int Clamp(int value) => Math.Clamp(value, 1, 10);
}
