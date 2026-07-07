using TxConcert.Domain.Common;
using TxConcert.Domain.Common.Base;
using NodaTime;

namespace TxConcert.Domain.Events;

/// <summary>
/// Stores events transactionally before publishing to subscribers.
/// Tracks which subscribers have processed the event via ProcessedBy list.
/// </summary>
public sealed class EventOutboxEntity : BaseEntity
{
    private EventOutboxEntity() { } // EF Core

    public static EventOutboxEntity Create(string eventName, string content, string? jobId = null)
    {
        var entity = new EventOutboxEntity
        {
            JobId = jobId ?? Guid.NewGuid().ToString("N"),
            EventName = eventName,
            Content = content,
            OccurredOn = SystemClock.Instance.GetCurrentInstant(),
            ProcessedBy = []
        };
        entity.SetId(Constants.IdPrefix.EventOutbox);
        return entity;
    }

    public string JobId { get; private set; } = default!;
    public string EventName { get; private set; } = default!;
    public string Content { get; private set; } = default!;
    public Instant OccurredOn { get; private set; }
    public Instant? PublishedOn { get; private set; }
    public Instant? ProcessedOn { get; private set; }
    public string? ErrorMessage { get; private set; }
    public bool? IsSuccessful { get; private set; }

    /// <summary>
    /// Subscriber IDs that have successfully processed this event.
    /// Stored as a JSONB array in PostgreSQL.
    /// </summary>
    public List<string> ProcessedBy { get; private set; } = [];

    public void SetPublished() => PublishedOn = SystemClock.Instance.GetCurrentInstant();

    public void SetProcessed() => ProcessedOn = SystemClock.Instance.GetCurrentInstant();

    public void SetSuccess()
    {
        IsSuccessful = true;
        SetProcessed();
    }

    public void SetFailed(string errorMessage)
    {
        IsSuccessful = false;
        ErrorMessage = errorMessage;
        SetProcessed();
    }

    public void MarkProcessedBy(string subscriberId)
    {
        if (!ProcessedBy.Contains(subscriberId))
            ProcessedBy.Add(subscriberId);
    }
}
