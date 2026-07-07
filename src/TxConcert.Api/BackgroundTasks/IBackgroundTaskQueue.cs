namespace TxConcert.Api.BackgroundTasks;

public interface IBackgroundTaskQueue
{
    ValueTask QueueAsync(
        Func<IServiceProvider, CancellationToken, ValueTask> workItem,
        CancellationToken cancellationToken = default);

    ValueTask<Func<IServiceProvider, CancellationToken, ValueTask>> DequeueAsync(
        CancellationToken cancellationToken);
}
