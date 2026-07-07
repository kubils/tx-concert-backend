namespace TxConcert.Domain.Common.Settings;

public sealed class AiSettings
{
    public const string SectionName = "Ai";

    public AiProvider Provider { get; init; } = AiProvider.OpenAi;
    public int BatchSize { get; init; } = 10;
}

public enum AiProvider
{
    Claude,
    OpenAi
}
