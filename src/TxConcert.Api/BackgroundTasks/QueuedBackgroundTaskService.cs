namespace TxConcert.Api.BackgroundTasks;

public sealed class QueuedBackgroundTaskService(
    IBackgroundTaskQueue taskQueue,
    IServiceScopeFactory scopeFactory,
    ILogger<QueuedBackgroundTaskService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Queued background task service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            Func<IServiceProvider, CancellationToken, ValueTask> workItem;

            try
            {
                workItem = await taskQueue.DequeueAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }

            try
            {
                await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
                await workItem(scope.ServiceProvider, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Queued background task failed");
            }
        }

        logger.LogInformation("Queued background task service stopped");
    }
}
