using TxConcert.Api.BackgroundTasks;
using TxConcert.Application.Common.Responses;
using TxConcert.Application.Features.Concerts.Commands.EvaluateConcertById;
using TxConcert.Application.Features.Concerts.Commands.EvaluateConcerts;
using TxConcert.Application.Features.Concerts.Commands.SyncConcerts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TxConcert.Api.Controllers;

[ApiController]
[Route("api/test")]
[Authorize(AuthenticationSchemes = "ApiKey")]
public sealed class TestSyncController(
    IBackgroundTaskQueue backgroundTaskQueue,
    ILogger<TestSyncController> logger) : ControllerBase
{
    /// <summary>
    /// Manually trigger Ticketmaster concert sync.
    /// Will be replaced by a recurring job later.
    /// </summary>
    [HttpPost("sync-concerts")]
    public async Task<ActionResult<ApiResponse<BackgroundJobResponse>>> SyncConcerts(CancellationToken ct)
    {
        var jobId = Guid.NewGuid();

        await backgroundTaskQueue.QueueAsync(async (serviceProvider, backgroundCt) =>
        {
            ILogger<TestSyncController> backgroundLogger =
                serviceProvider.GetRequiredService<ILogger<TestSyncController>>();
            ISender scopedMediator = serviceProvider.GetRequiredService<ISender>();

            backgroundLogger.LogInformation("Starting queued Ticketmaster concert sync {JobId}", jobId);

            SyncConcertsResult result = await scopedMediator.Send(new SyncConcertsCommand(), backgroundCt);

            backgroundLogger.LogInformation(
                "Queued Ticketmaster concert sync {JobId} complete: {Created} created, {Updated} updated, {Skipped} skipped",
                jobId,
                result.Created,
                result.Updated,
                result.Skipped);
        }, ct);

        logger.LogInformation("Queued Ticketmaster concert sync {JobId}", jobId);

        return Accepted(ApiResponse<BackgroundJobResponse>.Ok(
            new BackgroundJobResponse(jobId, "Queued"),
            "Ticketmaster concert sync queued."));
    }

    /// <summary>
    /// Manually trigger AI evaluation for unevaluated concerts.
    /// </summary>
    [HttpPost("evaluate-concerts")]
    public async Task<ActionResult<ApiResponse<BackgroundJobResponse>>> EvaluateConcerts(CancellationToken ct)
    {
        var jobId = Guid.NewGuid();

        await backgroundTaskQueue.QueueAsync(async (serviceProvider, backgroundCt) =>
        {
            ILogger<TestSyncController> backgroundLogger =
                serviceProvider.GetRequiredService<ILogger<TestSyncController>>();
            ISender scopedMediator = serviceProvider.GetRequiredService<ISender>();

            backgroundLogger.LogInformation("Starting queued AI concert evaluation {JobId}", jobId);

            EvaluateConcertsResult result = await scopedMediator.Send(new EvaluateConcertsCommand(), backgroundCt);

            backgroundLogger.LogInformation(
                "Queued AI concert evaluation {JobId} complete: {Evaluated} evaluated, {Skipped} skipped, {Failed} failed",
                jobId,
                result.Evaluated,
                result.Skipped,
                result.Failed);
        }, ct);

        logger.LogInformation("Queued AI concert evaluation {JobId}", jobId);

        return Accepted(ApiResponse<BackgroundJobResponse>.Ok(
            new BackgroundJobResponse(jobId, "Queued"),
            "AI concert evaluation queued."));
    }

    /// <summary>
    /// Evaluate a specific concert with AI by its ID.
    /// </summary>
    [HttpPost("evaluate-concert/{concertId}")]
    public async Task<ActionResult<ApiResponse<BackgroundJobResponse>>> EvaluateConcertById(
        string concertId, CancellationToken ct)
    {
        Guid jobId = Guid.NewGuid();

        await backgroundTaskQueue.QueueAsync(async (serviceProvider, backgroundCt) =>
        {
            ILogger<TestSyncController> backgroundLogger =
                serviceProvider.GetRequiredService<ILogger<TestSyncController>>();
            ISender scopedMediator = serviceProvider.GetRequiredService<ISender>();

            backgroundLogger.LogInformation(
                "Starting queued AI concert evaluation {JobId} for concert {ConcertId}",
                jobId,
                concertId);

            EvaluateConcertByIdResult result = await scopedMediator.Send(
                new EvaluateConcertByIdCommand(concertId),
                backgroundCt);

            backgroundLogger.LogInformation(
                "Queued AI concert evaluation {JobId} for concert {ConcertId} complete: article generated {ArticleGenerated}",
                jobId,
                concertId,
                result.ArticleGenerated);
        }, ct);

        logger.LogInformation(
            "Queued AI concert evaluation {JobId} for concert {ConcertId}",
            jobId,
            concertId);

        return Accepted(ApiResponse<BackgroundJobResponse>.Ok(
            new BackgroundJobResponse(jobId, "Queued"),
            "AI concert evaluation queued."));
    }
}
