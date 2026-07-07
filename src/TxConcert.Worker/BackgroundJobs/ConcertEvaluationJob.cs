using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using TxConcert.Application.Features.Concerts.Commands.EvaluateConcerts;

namespace TxConcert.Worker.BackgroundJobs;

/// <summary>
/// Runs after concert sync to evaluate unevaluated concerts with AI.
/// </summary>
[DisallowConcurrentExecution]
public sealed class ConcertEvaluationJob(
    IServiceScopeFactory scopeFactory,
    ILogger<ConcertEvaluationJob> logger) : IJob
{
    public const string JobKey = "concert-evaluation";

    public async Task Execute(IJobExecutionContext context)
    {
        logger.LogInformation("Starting AI concert evaluation");

        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
        ISender mediator = scope.ServiceProvider.GetRequiredService<ISender>();

        EvaluateConcertsResult result = await mediator.Send(
            new EvaluateConcertsCommand(), context.CancellationToken);

        logger.LogInformation(
            "AI evaluation complete: {Evaluated} evaluated, {Skipped} skipped, {Failed} failed",
            result.Evaluated, result.Skipped, result.Failed);
    }
}
