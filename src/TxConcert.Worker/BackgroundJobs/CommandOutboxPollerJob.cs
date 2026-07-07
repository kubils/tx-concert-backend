using TxConcert.Domain.Commands;
using TxConcert.Infrastructure.Messaging.Commands;
using TxConcert.Infrastructure.Persistence.Repositories;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

namespace TxConcert.Worker.BackgroundJobs;

[DisallowConcurrentExecution]
public sealed class CommandOutboxPollerJob(
    IServiceScopeFactory scopeFactory,
    ILogger<CommandOutboxPollerJob> logger) : IJob
{
    public const string JobKey = "command-outbox-poller";

    public async Task Execute(IJobExecutionContext context)
    {
        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
        CommandOutboxRepository repository = scope.ServiceProvider.GetRequiredService<CommandOutboxRepository>();
        IPublishEndpoint publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        IReadOnlyList<CommandOutboxEntity> unpublished =
            await repository.GetUnpublishedAsync(ct: context.CancellationToken);

        if (unpublished.Count == 0)
            return;

        logger.LogInformation("Publishing {Count} commands from outbox", unpublished.Count);

        foreach (CommandOutboxEntity entry in unpublished)
        {
            try
            {
                await publishEndpoint.Publish(
                    new CommandMessage(entry.Id, entry.Command, entry.Content),
                    context.CancellationToken);

                entry.SetPublished();
                await repository.UpsertAsync(entry, context.CancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to publish command {CommandId}", entry.Id);
            }
        }
    }
}
