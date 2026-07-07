using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using TxConcert.Application.Features.Concerts.Commands.SyncConcerts;

namespace TxConcert.Worker.BackgroundJobs;

/// <summary>
/// Daily job that syncs upcoming concerts from Ticketmaster.
/// </summary>
[DisallowConcurrentExecution]
public sealed class ConcertSyncJob(
    IServiceScopeFactory scopeFactory,
    ILogger<ConcertSyncJob> logger) : IJob
{
    public const string JobKey = "concert-sync";

    public async Task Execute(IJobExecutionContext context)
    {
        logger.LogInformation("Starting daily concert sync from Ticketmaster");

        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
        ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();

        SyncConcertsResult result = await mediator.Send(new SyncConcertsCommand(), context.CancellationToken);

        logger.LogInformation(
            "Concert sync complete: {Created} created, {Updated} updated, {Skipped} skipped",
            result.Created, result.Updated, result.Skipped);
    }
}
