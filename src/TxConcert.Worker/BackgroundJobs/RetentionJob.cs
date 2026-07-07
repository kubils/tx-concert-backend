using TxConcert.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NodaTime;
using Quartz;

namespace TxConcert.Worker.BackgroundJobs;

/// <summary>
/// Periodically cleans up old successful outbox entries.
/// </summary>
[DisallowConcurrentExecution]
public sealed class RetentionJob(
    IServiceScopeFactory scopeFactory,
    IClock clock,
    ILogger<RetentionJob> logger) : IJob
{
    public const string JobKey = "outbox-retention";
    private static readonly Duration RetentionPeriod = Duration.FromDays(7);

    public async Task Execute(IJobExecutionContext context)
    {
        Instant threshold = clock.GetCurrentInstant() - RetentionPeriod;

        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
        CommandOutboxRepository commandRepo = scope.ServiceProvider.GetRequiredService<CommandOutboxRepository>();
        EventOutboxRepository eventRepo = scope.ServiceProvider.GetRequiredService<EventOutboxRepository>();

        int commandsDeleted = await commandRepo.CleanupSuccessfulAsync(threshold, context.CancellationToken);
        int eventsDeleted = await eventRepo.CleanupSuccessfulAsync(threshold, context.CancellationToken);

        if (commandsDeleted > 0 || eventsDeleted > 0)
            logger.LogInformation("Retention cleanup: {Commands} commands, {Events} events removed",
                commandsDeleted, eventsDeleted);
    }
}
