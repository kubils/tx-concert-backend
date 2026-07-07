namespace TxConcert.Infrastructure.Messaging.Commands;

public sealed record CommandMessage(
    string OutboxId,
    string Command,
    string Content);
