using TxConcert.Domain.Events;
using TxConcert.Infrastructure.Messaging.Events;
using TxConcert.Infrastructure.Persistence.Repositories;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

namespace TxConcert.Worker.BackgroundJobs;

[DisallowConcurrentExecution]
public sealed class EventOutboxPollerJob(
    IServiceScopeFactory scopeFactory,
    ILogger<EventOutboxPollerJob> logger) : IJob
{
    public const string JobKey = "event-outbox-poller";

    public async Task Execute(IJobExecutionContext context)
    {
        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
        EventOutboxRepository repository = scope.ServiceProvider.GetRequiredService<EventOutboxRepository>();
        IPublishEndpoint publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        IReadOnlyList<EventOutboxEntity> unpublished =
            await repository.GetUnpublishedAsync(ct: context.CancellationToken);

        if (unpublished.Count == 0)
            return;

        logger.LogInformation("Publishing {Count} events from outbox", unpublished.Count);

        foreach (EventOutboxEntity entry in unpublished)
        {
            try
            {
                await publishEndpoint.Publish(
                    new EventMessage(entry.Id, entry.EventName, entry.Content),
                    context.CancellationToken);

                entry.SetPublished();
                await repository.UpsertAsync(entry, context.CancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to publish event {EventId}", entry.Id);
            }
        }
    }
}
