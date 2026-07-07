using TxConcert.Domain.Common.Cqrs;

namespace TxConcert.Domain.Commands;

/// <summary>
/// Abstraction for sending commands.
/// In API mode: saves to outbox for later publishing to RabbitMQ via MassTransit.
/// In Worker mode: dispatches directly via MediatR.
/// Only accepts types implementing ICommand for compile-time safety.
/// </summary>
public interface ICommandBus
{
    Task SendAsync<T>(T command, CancellationToken ct = default) where T : class, ICommand;
    Task SendManyAsync<T>(IEnumerable<T> commands, CancellationToken ct = default) where T : class, ICommand;
}
