using MediatR;
using NodaTime;

namespace TxConcert.Domain.Common.Cqrs;

/// <summary>
/// Marker interface for domain events.
/// Published via MediatR INotification — all registered handlers receive the event.
/// NestJS equivalent: EventData + EventSubscriber pattern.
/// </summary>
public interface IDomainEvent : INotification
{
    Instant OccurredAt { get; }
}
