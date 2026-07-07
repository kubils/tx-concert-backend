using TxConcert.Domain.Common.Cqrs;
using TxConcert.Domain.Events;
using TxConcert.Infrastructure.Persistence;
using System.Text.Json;

namespace TxConcert.Infrastructure.Messaging.Events;

public sealed class QueueEventBusService(ApplicationDbContext context) : IEventBus
{
    public async Task PublishAsync<T>(T @event, CancellationToken ct = default) where T : class, IDomainEvent
    {
        string eventName = typeof(T).Name;
        EventOutboxEntity outbox = EventOutboxEntity.Create(
            eventName,
            JsonSerializer.Serialize(@event));

        await context.EventOutbox.AddAsync(outbox, ct);
        await context.SaveChangesAsync(ct);
    }
}
