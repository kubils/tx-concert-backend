using TxConcert.Domain.Common;
using TxConcert.Domain.Common.Base;
using NodaTime;

namespace TxConcert.Domain.Commands;

/// <summary>
/// Stores commands transactionally before publishing to the message queue.
/// Enables at-least-once delivery guarantee via outbox pattern.
/// NestJS prefix: cdout (5 chars)
/// </summary>
public sealed class CommandOutboxEntity : BaseEntity
{
    private CommandOutboxEntity() { } // EF Core

    public static CommandOutboxEntity Create(string commandName, string content, string? jobId = null)
    {
        var entity = new CommandOutboxEntity
        {
            JobId = jobId ?? Guid.NewGuid().ToString("N"),
            Command = commandName,
            Content = content,
            OccurredOn = SystemClock.Instance.GetCurrentInstant(),
            IsRetry = false
        };
        entity.SetId(Constants.IdPrefix.CommandOutbox);
        return entity;
    }

    public string JobId { get; private set; } = default!;
    public string Command { get; private set; } = default!;
    public string Content { get; private set; } = default!;
    public Instant OccurredOn { get; private set; }
    public Instant? PublishedOn { get; private set; }
    public Instant? ProcessedOn { get; private set; }
    public string? ErrorMessage { get; private set; }
    public bool? IsSuccessful { get; private set; }
    public bool IsRetry { get; private set; }

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

    /// <summary>Creates a new retry command entry based on this one.</summary>
    public CommandOutboxEntity Retry()
    {
        var retry = Create(Command, Content, JobId);
        retry.IsRetry = true;
        return retry;
    }
}
