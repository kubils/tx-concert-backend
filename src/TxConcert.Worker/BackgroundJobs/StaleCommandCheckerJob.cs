using TxConcert.Domain.Commands;
using TxConcert.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NodaTime;
using Quartz;

namespace TxConcert.Worker.BackgroundJobs;

/// <summary>
/// Detects commands that were published but never processed (stale).
/// Retries them up to their max retry count.
/// </summary>
[DisallowConcurrentExecution]
public sealed class StaleCommandCheckerJob(
    IServiceScopeFactory scopeFactory,
    IClock clock,
    ILogger<StaleCommandCheckerJob> logger) : IJob
{
    public const string JobKey = "stale-command-checker";
    private static readonly Duration StaleThreshold = Duration.FromMinutes(5);

    public async Task Execute(IJobExecutionContext context)
    {
        Instant threshold = clock.GetCurrentInstant() - StaleThreshold;

        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
        CommandOutboxRepository repository = scope.ServiceProvider.GetRequiredService<CommandOutboxRepository>();

        IReadOnlyList<CommandOutboxEntity> staleCommands =
            await repository.GetStaleProcessingAsync(threshold, context.CancellationToken);

        if (staleCommands.Count == 0)
            return;

        logger.LogWarning("Found {Count} stale commands, retrying", staleCommands.Count);

        foreach (CommandOutboxEntity entry in staleCommands)
        {
            entry.Retry();
            await repository.UpsertAsync(entry, context.CancellationToken);
        }
    }
}
