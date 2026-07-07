namespace TxConcert.Domain.Common.Settings;

public sealed class CommandQueueSettings
{
    public const string SectionName = "CommandQueue";

    public int Attempts { get; init; } = 3;
    public string BackoffType { get; init; } = "Exponential";
    public int BackoffDelayInMs { get; init; } = 5_000;
    public int OutboxProcessIntervalInMs { get; init; } = 3_000;
    public int OutboxProcessRetrievalLimit { get; init; } = 1_000;
}
