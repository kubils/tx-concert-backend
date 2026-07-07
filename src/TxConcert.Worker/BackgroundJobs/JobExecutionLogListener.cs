using TxConcert.Domain.Jobs;
using TxConcert.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NodaTime;
using Quartz;

namespace TxConcert.Worker.BackgroundJobs;

public sealed class JobExecutionLogListener(
    IServiceScopeFactory scopeFactory,
    IClock clock,
    ILogger<JobExecutionLogListener> logger) : IJobListener
{
    public string Name => nameof(JobExecutionLogListener);

    public async Task JobToBeExecuted(IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        JobExecutionLogEntity log = JobExecutionLogEntity.Start(
            context.JobDetail.Key.Name,
            context.JobDetail.Key.Group,
            context.Trigger.Key.Name,
            context.Trigger.Key.Group,
            context.FireInstanceId,
            clock.GetCurrentInstant());

        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
        JobExecutionLogRepository repository = scope.ServiceProvider.GetRequiredService<JobExecutionLogRepository>();
        await repository.UpsertAsync(log, cancellationToken);

        logger.LogInformation("Quartz job started: {JobName} ({FireInstanceId})",
            log.JobName,
            log.FireInstanceId);
    }

    public Task JobExecutionVetoed(
        IJobExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public async Task JobWasExecuted(
        IJobExecutionContext context,
        JobExecutionException? jobException,
        CancellationToken cancellationToken = default)
    {
        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
        JobExecutionLogRepository repository = scope.ServiceProvider.GetRequiredService<JobExecutionLogRepository>();

        JobExecutionLogEntity? log = await repository.GetRunningByFireInstanceIdAsync(
            context.FireInstanceId,
            cancellationToken);

        if (log is null)
        {
            logger.LogWarning("Quartz job log not found for fire instance {FireInstanceId}",
                context.FireInstanceId);
            return;
        }

        Instant completedAt = clock.GetCurrentInstant();
        if (jobException is null)
        {
            log.Complete(completedAt);
            logger.LogInformation("Quartz job completed: {JobName} ({FireInstanceId}) in {DurationMs} ms",
                log.JobName,
                log.FireInstanceId,
                log.DurationMs);
        }
        else
        {
            log.Fail(completedAt, jobException.ToString());
            logger.LogError(jobException, "Quartz job failed: {JobName} ({FireInstanceId})",
                log.JobName,
                log.FireInstanceId);
        }

        await repository.UpsertAsync(log, cancellationToken);
    }
}
