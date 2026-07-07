namespace TxConcert.Domain.Common.Settings;

public sealed class EventQueueSettings
{
    public const string SectionName = "EventQueue";

    public int Attempts { get; init; } = 1;
    public string BackoffType { get; init; } = "Exponential";
    public int BackoffDelayInMs { get; init; } = 1_000;
    public int RemoveOnCompleteCount { get; init; } = 100;
    public int RemoveOnCompleteAgeInMs { get; init; } = 86_400_000; // 1 day
    public int RemoveOnFailCount { get; init; } = 500;
    public int RemoveOnFailAgeInMs { get; init; } = 604_800_000; // 7 days
    public int OutboxProcessIntervalInMs { get; init; } = 3_000;
    public int OutboxProcessRetrievalLimit { get; init; } = 100;
}
