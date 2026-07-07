namespace TxConcert.Domain.Cache;

/// <summary>
/// Redis-backed cache service with distributed locking support.
/// </summary>
public interface ICacheService
{
    // Basic cache operations
    Task<T?> GetAsync<T>(string key, CancellationToken ct = default) where T : class;
    Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken ct = default) where T : class;
    Task DeleteAsync(string key, CancellationToken ct = default);
    Task DeleteByPatternAsync(string pattern, CancellationToken ct = default);
    Task<bool> ExistsAsync(string key, CancellationToken ct = default);
    Task<bool> ExpireAsync(string key, TimeSpan ttl, CancellationToken ct = default);

    // Set operations
    Task AddToSetAsync(string key, string value, CancellationToken ct = default);
    Task<bool> RemoveFromSetAsync(string key, string value, CancellationToken ct = default);
    Task<bool> IsSetMemberAsync(string key, string value, CancellationToken ct = default);
    Task<IReadOnlySet<string>> GetSetMembersAsync(string key, CancellationToken ct = default);

    // Distributed lock — returns null if lock cannot be acquired
    Task<IDistributedLock?> AcquireLockAsync(
        string resource,
        TimeSpan duration,
        CancellationToken ct = default);

    Task<bool> PingAsync(CancellationToken ct = default);
}
