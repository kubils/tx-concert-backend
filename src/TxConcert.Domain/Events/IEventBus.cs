using TxConcert.Domain.Common.Cqrs;

namespace TxConcert.Domain.Events;

/// <summary>
/// Abstraction for publishing domain events.
/// Saves to outbox for later publishing to RabbitMQ via MassTransit.
/// Only accepts types implementing IDomainEvent for compile-time safety.
/// </summary>
public interface IEventBus
{
    Task PublishAsync<T>(T @event, CancellationToken ct = default) where T : class, IDomainEvent;
}
