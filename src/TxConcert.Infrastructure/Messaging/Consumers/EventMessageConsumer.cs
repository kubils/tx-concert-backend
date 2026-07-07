using TxConcert.Domain.Events;
using TxConcert.Infrastructure.Messaging.Events;
using TxConcert.Infrastructure.Persistence;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace TxConcert.Infrastructure.Messaging.Consumers;

/// <summary>
/// MassTransit consumer for processing events from RabbitMQ.
/// Resolves handlers via EventSubscriberRegistry and updates the outbox record.
/// NestJS equivalent: BullMQ worker processor for events.
/// </summary>
public sealed class EventMessageConsumer(
    IServiceScopeFactory scopeFactory,
    EventSubscriberRegistry registry,
    ILogger<EventMessageConsumer> logger)
    : IConsumer<EventMessage>
{
    public async Task Consume(ConsumeContext<EventMessage> context)
    {
        EventMessage message = context.Message;
        logger.LogInformation("Processing event {EventName} (outbox: {OutboxId})",
            message.EventName, message.OutboxId);

        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
        ApplicationDbContext db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        EventOutboxEntity? outbox = await db.EventOutbox.FindAsync([message.OutboxId], context.CancellationToken);
        if (outbox is null)
        {
            logger.LogWarning("Event outbox {OutboxId} not found, skipping", message.OutboxId);
            return;
        }

        try
        {
            await registry.HandleAsync(message.EventName, message.Content, scope.ServiceProvider, context.CancellationToken);
            string subscriberId = $"{message.EventName}-{nameof(EventMessageConsumer)}";
            outbox.MarkProcessedBy(subscriberId);
            logger.LogInformation("Event {EventName} processed successfully", message.EventName);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Event {EventName} failed", message.EventName);
            outbox.SetFailed(ex.Message);
        }

        await db.SaveChangesAsync(context.CancellationToken);
    }
}
