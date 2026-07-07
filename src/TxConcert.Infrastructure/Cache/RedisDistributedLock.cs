using TxConcert.Domain.Cache;
using RedLockNet;

namespace TxConcert.Infrastructure.Cache;

public sealed class RedisDistributedLock(IRedLock redLock) : IDistributedLock
{
    public bool IsAcquired => redLock.IsAcquired;
    public string Resource => redLock.Resource;

    public async ValueTask DisposeAsync()
    {
        await redLock.DisposeAsync();
    }
}
