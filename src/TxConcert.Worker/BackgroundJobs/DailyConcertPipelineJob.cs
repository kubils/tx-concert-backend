using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using TxConcert.Application.Features.Concerts.Commands.EvaluateConcerts;
using TxConcert.Application.Features.Concerts.Commands.SyncConcerts;

namespace TxConcert.Worker.BackgroundJobs;

/// <summary>
/// Daily pipeline that syncs a limited concert batch, then evaluates the changed concerts with AI.
/// </summary>
[DisallowConcurrentExecution]
public sealed class DailyConcertPipelineJob(
    IServiceScopeFactory scopeFactory,
    ILogger<DailyConcertPipelineJob> logger) : IJob
{
    public const string JobKey = "daily-concert-pipeline";
    private const int DailyBatchSize = 20;

    public async Task Execute(IJobExecutionContext context)
    {
        logger.LogInformation(
            "Starting daily concert pipeline with batch size {BatchSize}",
            DailyBatchSize);

        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
        ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();

        SyncConcertsResult syncResult = await mediator.Send(
            new SyncConcertsCommand(DailyBatchSize),
            context.CancellationToken);

        logger.LogInformation(
            "Concert sync complete: {Created} created, {Updated} updated, {Skipped} skipped, {NeedsEvaluation} queued for evaluation",
            syncResult.Created,
            syncResult.Updated,
            syncResult.Skipped,
            syncResult.EvaluationConcertIds?.Count ?? 0);

        if (syncResult.EvaluationConcertIds is not { Count: > 0 })
        {
            logger.LogInformation("Daily concert pipeline finished with no concerts requiring AI evaluation");
            return;
        }

        EvaluateConcertsResult evaluationResult = await mediator.Send(
            new EvaluateConcertsCommand(syncResult.EvaluationConcertIds, DailyBatchSize),
            context.CancellationToken);

        logger.LogInformation(
            "Daily concert pipeline finished: {Evaluated} evaluated, {Skipped} skipped, {Failed} failed, {ArticlesGenerated} articles, {ArticlesFailed} article failures",
            evaluationResult.Evaluated,
            evaluationResult.Skipped,
            evaluationResult.Failed,
            evaluationResult.ArticlesGenerated,
            evaluationResult.ArticlesFailed);
    }
}
