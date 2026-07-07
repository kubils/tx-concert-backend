namespace TxConcert.Infrastructure.Messaging.Events;

public sealed record EventMessage(
    string OutboxId,
    string EventName,
    string Content);
