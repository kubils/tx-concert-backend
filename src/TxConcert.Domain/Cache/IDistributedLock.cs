namespace TxConcert.Domain.Cache;

/// <summary>
/// Represents an acquired distributed lock that must be released.
/// Use within a using block.
/// </summary>
public interface IDistributedLock : IAsyncDisposable
{
    bool IsAcquired { get; }
    string Resource { get; }
}
