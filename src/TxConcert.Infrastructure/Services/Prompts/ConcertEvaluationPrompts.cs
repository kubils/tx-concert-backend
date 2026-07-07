using TxConcert.Domain.Features.Concerts;

namespace TxConcert.Infrastructure.Services.Prompts;

internal static class ConcertEvaluationPrompts
{
    public const string DefaultUserPromptInstructions =
        "For each generated artistVibes entry, write summary as at least 3 paragraphs separated with \\n\\n.";

    public const string DefaultSystemPrompt = """
            You are a Texas concert expert and music critic. You evaluate concerts happening in Texas
            and provide engaging descriptions and vibe scores.

            You MUST respond with ONLY valid JSON (no markdown, no code fences, no extra text).

            JSON schema:
            {
              "description": "2-3 paragraph engaging concert description for fans. Include what to expect, artist highlights, venue atmosphere. Write in an exciting, informative tone.",
              "concertVibes": {
                "energy": <1-10>,
                "soundQuality": <1-10>,
                "hype": <1-10>,
                "crowdVibe": <1-10>,
                "valueForMoney": <1-10>,
                "summary": "1-2 sentence vibe summary"
              },
              "artistVibes": [
                {
                  "artistName": "exact artist name from input",
                  "visuals": <1-10>,
                  "sound": <1-10>,
                  "energy": <1-10>,
                  "fanInteraction": <1-10>,
                  "texasSpirit": <1-10>,
                  "summary": "minimum 3 paragraphs artist interpretation for Texas fans, as a single JSON string with \\n\\n between paragraphs"
                }
              ]
            }

            Scoring guidelines:
            - energy: How energetic is the live performance expected to be?
            - soundQuality: Known quality of live sound production
            - hype: Current buzz and anticipation level
            - crowdVibe: Expected crowd atmosphere and engagement
            - valueForMoney: Price vs expected experience quality
            - visuals: Stage production, lighting, visual effects
            - sound: Artist's sonic quality and musical skill
            - fanInteraction: How much the artist engages with fans
            - texasSpirit: Connection to Texas culture, history of Texas shows, local love

            Every artistVibes.summary MUST be at least 3 paragraphs separated by blank lines (use \n\n inside the JSON string).
            Cover: live performance style, fan/crowd expectations, and why this artist matters for a Texas concert audience.

            If you don't know enough about an artist, give moderate scores (5-6) and still write at least 3 cautious paragraphs in the summary.

            When web research is provided, use it to inform your scoring with higher confidence.
            Prioritize real reviews, fan experiences, and venue details from the research data.
            """;

    public static string BuildSystemPrompt()
    {
        return DefaultSystemPrompt;
    }

    public static string BuildUserPrompt(
        ConcertEvaluationInput input,
        string? userPromptInstructions = null)
    {
        var artistsNeedingVibes = input.Artists.Where(a => !a.HasExistingVibeProfile).ToList();
        var artistsWithVibes = input.Artists.Where(a => a.HasExistingVibeProfile).ToList();

        string artists = string.Join("\n", input.Artists.Select(a =>
            $"  - {a.Name} (Genre: {a.GenreName ?? "unknown"}, Headliner: {a.IsHeadliner})" +
            (a.HasExistingVibeProfile ? " [VIBE PROFILE EXISTS — skip artistVibes for this artist]" : "") +
            (a.Bio != null ? $"\n    Bio: {a.Bio}" : "")));

        string price = input.PriceMin.HasValue || input.PriceMax.HasValue
            ? $"${input.PriceMin ?? 0} - ${input.PriceMax ?? 0}"
            : "Not available";

        string vibeInstruction = artistsWithVibes.Count > 0
            ? $"\n\nIMPORTANT: Only generate artistVibes entries for artists that do NOT have [VIBE PROFILE EXISTS] tag. " +
              $"Skip these artists in artistVibes: {string.Join(", ", artistsWithVibes.Select(a => a.Name))}. " +
              $"Still include ALL artists in the concert description text."
            : "";

        return $"""
            Evaluate this Texas concert:

            Concert: {input.ConcertName}
            Date: {input.EventDate}
            Time: {input.StartTime ?? "TBA"}
            Venue: {input.VenueName ?? "Unknown"}, {input.VenueCity ?? "Texas"}
            Genre: {input.GenreName ?? "Unknown"} / {input.SubGenreName ?? ""}
            Price Range: {price}
            {(input.Info != null ? $"Info: {input.Info}" : "")}
            {(input.PleaseNote != null ? $"Note: {input.PleaseNote}" : "")}

            Artists:
            {artists}
            {BuildWebResearchSection(input.WebResearch)}{vibeInstruction}
            {userPromptInstructions ?? DefaultUserPromptInstructions}
            Respond with ONLY valid JSON.
            """;
    }

    private static string BuildWebResearchSection(Dictionary<string, string> webResearch)
    {
        if (webResearch.Count == 0) return "";

        var sb = new System.Text.StringBuilder();
        sb.AppendLine();
        sb.AppendLine("            Web Research:");
        foreach ((string key, string value) in webResearch)
        {
            sb.AppendLine($"            [{key}]");
            sb.AppendLine($"            {value}");
        }

        return sb.ToString();
    }
}
