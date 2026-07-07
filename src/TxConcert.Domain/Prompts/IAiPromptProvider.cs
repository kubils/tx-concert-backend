namespace TxConcert.Domain.Prompts;

public interface IAiPromptProvider
{
    Task<AiPromptContent> GetConcertEvaluationPromptAsync(CancellationToken ct = default);
}

public sealed record AiPromptContent(
    string SystemPrompt,
    string UserPromptInstructions,
    string Source,
    int? VersionNumber);
