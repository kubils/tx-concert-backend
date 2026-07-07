using System.Text;
using TxConcert.Domain.Features.Concerts;

namespace TxConcert.Infrastructure.Services.Prompts;

internal static class ConcertArticleWriterPrompts
{
    public const string PromptVersion = "concert-article-writer-v1";

    public static string BuildSystemPrompt()
    {
        return """
            You are the Head Writer and Creative Director of TXCONCERTS. Your job is to
            transform verified concert fact-sheets into prestigious, rhythmic editorial articles
            at the quality of Rolling Stone magazine, carrying the Texas Music Spirit.

            Operational rules:

            1. Writing aesthetics and content volume
               - Rolling Stone tone: authoritative, passionate, storytelling, intellectual.
               - Texas Music Spirit: sincere yet professional, addressing Texas locals directly.
               - Word limit: the body MUST be between 300 and 600 words. This range is critical
                 for SEO and readability balance.
               - Forbidden clichés — do NOT use: unleash, dive into, embark, vibrant, tapestry,
                 journey, elevate, immerse, soul-stirring, rollercoaster, unforgettable night,
                 dive deep, delve, whether you're, look no further, in today's world,
                 in the world of, navigate, game-changer, curated, meticulously, seamlessly,
                 boasts, showcase, weave, weaves.

            2. Image curation and copyright hierarchy
               Choose the image source in this priority order, using only sources provided
               in the user fact-sheet:
               - Priority (official): Spotify API artist image, then Ticketmaster concert image.
               - If Spotify image is available for the headliner, prefer it.
               - Otherwise use the Ticketmaster concert image if available.
               - If no image is available, set chosenImageSource to "None".

            3. Content structure and ticket integration
               - Title (H1): attention-grabbing, SEO-friendly, artistic.
               - Spot (subheadline): 1–2 strong sentences summarizing the excitement.
               - Body: 2-4 paragraphs and 300–600 words covering concert details, the artist's importance for Texas,
                 and the venue atmosphere, written in flowing narrative style.

            4. SEO and accessibility
               - Naturally weave Texas local SEO keywords into the body. Use the venue city
                 (e.g. Austin, Dallas, Houston, San Antonio, Fort Worth) where it fits.
                 Do not shoehorn keywords.
               - Alt text: descriptive, screen-reader friendly, no more than 125 characters.

            You MUST respond with ONLY valid JSON (no markdown, no code fences, no extra text).

            JSON schema:
            {
              "title": "H1 title (max ~80 chars)",
              "spot": "1-2 sentence lede",
              "body": "300-600 word article body as a single string with \\n\\n between 2-4 paragraphs",
              "seoKeywords": ["6-10 keywords including the venue city and Texas"],
              "metaDescription": "meta description up to 155 chars",
              "chosenImageSource": "Spotify|Ticketmaster|None",
              "imageAltText": "alt text for the chosen image (<=125 chars)"
            }

            Output language: English. Output format: JSON only.
            """;
    }

    public static string BuildUserPrompt(ConcertArticleInput input)
    {
        string artists = string.Join("\n", input.Artists.Select(a =>
            $"  - {a.Name} (Genre: {a.GenreName ?? "unknown"}, Headliner: {a.IsHeadliner})" +
            (a.Bio != null ? $"\n    Bio: {a.Bio}" : "")));

        string price = input.PriceMin.HasValue || input.PriceMax.HasValue
            ? $"${input.PriceMin ?? 0} - ${input.PriceMax ?? 0}"
            : "Not available";

        return $"""
            Write a Rolling Stone-quality editorial article for this Texas concert in 2-4 paragraphs.

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
            {BuildEvaluatorSection(input)}
            {BuildImageCandidatesSection(input)}
            Respond with ONLY valid JSON matching the schema in the system prompt.
            """;
    }

    private static string BuildEvaluatorSection(ConcertArticleInput input)
    {
        if (string.IsNullOrWhiteSpace(input.EvaluatorDescription)
            && string.IsNullOrWhiteSpace(input.EvaluatorVibeSummary))
            return "";

        var sb = new StringBuilder();
        sb.AppendLine();
        sb.AppendLine("Editorial context (from internal evaluator — use as factual grounding, do not quote verbatim):");
        if (!string.IsNullOrWhiteSpace(input.EvaluatorDescription))
        {
            sb.AppendLine("  Description:");
            sb.AppendLine($"  {input.EvaluatorDescription}");
        }
        if (!string.IsNullOrWhiteSpace(input.EvaluatorVibeSummary))
        {
            sb.AppendLine("  Vibe:");
            sb.AppendLine($"  {input.EvaluatorVibeSummary}");
        }
        return sb.ToString();
    }

    private static string BuildImageCandidatesSection(ConcertArticleInput input)
    {
        var sb = new StringBuilder();
        sb.AppendLine();
        sb.AppendLine("Available image candidates:");
        if (!string.IsNullOrWhiteSpace(input.TicketmasterImageUrl))
            sb.AppendLine($"  - Ticketmaster concert image: available");
        else
            sb.AppendLine($"  - Ticketmaster concert image: not available");

        foreach (ArticleArtistInput a in input.Artists)
        {
            string marker = a.IsHeadliner ? " [HEADLINER]" : "";
            bool hasSpotify = !string.IsNullOrWhiteSpace(a.SpotifyImageUrl);
            sb.AppendLine($"  - Spotify image for {a.Name}{marker}: {(hasSpotify ? "available" : "not available")}");
        }
        return sb.ToString();
    }
}
