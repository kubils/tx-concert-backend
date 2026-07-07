using System.Threading.Channels;

namespace TxConcert.Api.BackgroundTasks;

public sealed class BackgroundTaskQueue : IBackgroundTaskQueue
{
    private readonly Channel<Func<IServiceProvider, CancellationToken, ValueTask>> queue =
        Channel.CreateUnbounded<Func<IServiceProvider, CancellationToken, ValueTask>>(
            new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false
            });

    public async ValueTask QueueAsync(
        Func<IServiceProvider, CancellationToken, ValueTask> workItem,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(workItem);

        await queue.Writer.WriteAsync(workItem, cancellationToken);
    }

    public async ValueTask<Func<IServiceProvider, CancellationToken, ValueTask>> DequeueAsync(
        CancellationToken cancellationToken)
    {
        return await queue.Reader.ReadAsync(cancellationToken);
    }
}
