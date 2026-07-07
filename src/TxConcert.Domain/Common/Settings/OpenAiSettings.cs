namespace TxConcert.Domain.Common.Settings;

public sealed class OpenAiSettings
{
    public const string SectionName = "Ai:OpenAi";

    public string ApiKey { get; init; } = "";
    public string Model { get; init; } = "gpt-4o";
    public int MaxTokens { get; init; } = 2048;
}
