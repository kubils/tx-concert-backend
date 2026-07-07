namespace TxConcert.Domain.Common.Settings;

public sealed class ClaudeSettings
{
    public const string SectionName = "Ai:Claude";

    public string ApiKey { get; init; } = "";
    public string Model { get; init; } = "claude-sonnet-4-20250514";
    public int MaxTokens { get; init; } = 2048;
}
