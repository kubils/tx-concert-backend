using TxConcert.Domain.Cache;
using TxConcert.Domain.Common.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RedLockNet;
using RedLockNet.SERedis;
using StackExchange.Redis;
using System.Text.Json;

namespace TxConcert.Infrastructure.Cache;

public sealed class RedisCacheService : ICacheService, IAsyncDisposable
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _db;
    private readonly RedLockFactory _redLockFactory;
    private readonly string _prefix;
    private readonly ILogger<RedisCacheService> _logger;

    public RedisCacheService(
        IConnectionMultiplexer redis,
        IOptions<RedisSettings> settings,
        ILogger<RedisCacheService> logger)
    {
        _redis = redis;
        _db = redis.GetDatabase();
        _prefix = settings.Value.KeyPrefix ?? "";
        _logger = logger;
        _redLockFactory = RedLockFactory.Create(
            [new RedLockNet.SERedis.Configuration.RedLockMultiplexer(redis)]);
    }

    private string Key(string key) => string.IsNullOrEmpty(_prefix) ? key : $"{_prefix}:{key}";

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default) where T : class
    {
        string cacheKey = Key(key);
        RedisValue value = await _db.StringGetAsync(cacheKey);

        if (value.IsNullOrEmpty)
        {
            _logger.LogDebug("Redis cache miss for key {CacheKey}", cacheKey);
            return default;
        }

        _logger.LogDebug("Redis cache hit for key {CacheKey}", cacheKey);
        return JsonSerializer.Deserialize<T>(value!);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken ct = default) where T : class
    {
        string cacheKey = Key(key);
        string json = JsonSerializer.Serialize(value);
        await _db.StringSetAsync(cacheKey, json, ttl);
        _logger.LogDebug("Redis cache set for key {CacheKey} with TTL {TtlSeconds}s", cacheKey, ttl.TotalSeconds);
    }

    public async Task DeleteAsync(string key, CancellationToken ct = default)
    {
        string cacheKey = Key(key);
        bool deleted = await _db.KeyDeleteAsync(cacheKey);
        _logger.LogDebug("Redis cache delete for key {CacheKey} (deleted: {Deleted})", cacheKey, deleted);
    }

    public async Task DeleteByPatternAsync(string pattern, CancellationToken ct = default)
    {
        IServer server = _redis.GetServer(_redis.GetEndPoints().First());
        string cachePattern = Key(pattern);
        int deletedCount = 0;

        await foreach (RedisKey redisKey in server.KeysAsync(pattern: cachePattern))
        {
            if (await _db.KeyDeleteAsync(redisKey))
                deletedCount++;
        }

        _logger.LogInformation(
            "Redis cache pattern delete completed for {CachePattern}; deleted {DeletedCount} keys",
            cachePattern,
            deletedCount);
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken ct = default)
    {
        string cacheKey = Key(key);
        bool exists = await _db.KeyExistsAsync(cacheKey);
        _logger.LogDebug("Redis cache exists check for key {CacheKey}: {Exists}", cacheKey, exists);
        return exists;
    }

    public async Task<bool> ExpireAsync(string key, TimeSpan ttl, CancellationToken ct = default)
    {
        string cacheKey = Key(key);
        bool updated = await _db.KeyExpireAsync(cacheKey, ttl);
        _logger.LogDebug(
            "Redis cache expire update for key {CacheKey} (updated: {Updated}, ttl: {TtlSeconds}s)",
            cacheKey,
            updated,
            ttl.TotalSeconds);
        return updated;
    }

    public async Task AddToSetAsync(string key, string value, CancellationToken ct = default)
    {
        string cacheKey = Key(key);
        await _db.SetAddAsync(cacheKey, value);
        _logger.LogDebug("Redis set add for key {CacheKey}", cacheKey);
    }

    public async Task<bool> RemoveFromSetAsync(string key, string value, CancellationToken ct = default)
    {
        string cacheKey = Key(key);
        bool removed = await _db.SetRemoveAsync(cacheKey, value);
        _logger.LogDebug("Redis set remove for key {CacheKey} (removed: {Removed})", cacheKey, removed);
        return removed;
    }

    public async Task<bool> IsSetMemberAsync(string key, string value, CancellationToken ct = default)
    {
        string cacheKey = Key(key);
        bool contains = await _db.SetContainsAsync(cacheKey, value);
        _logger.LogDebug("Redis set membership check for key {CacheKey}: {Contains}", cacheKey, contains);
        return contains;
    }

    public async Task<IReadOnlySet<string>> GetSetMembersAsync(string key, CancellationToken ct = default)
    {
        string cacheKey = Key(key);
        RedisValue[] members = await _db.SetMembersAsync(cacheKey);
        _logger.LogDebug("Redis set members read for key {CacheKey}: {Count} members", cacheKey, members.Length);
        return new HashSet<string>(members.Select(m => m.ToString()));
    }

    public async Task<IDistributedLock?> AcquireLockAsync(
        string resource,
        TimeSpan duration,
        CancellationToken ct = default)
    {
        string lockResource = Key(resource);
        _logger.LogInformation(
            "Attempting to acquire distributed lock for resource {Resource} with duration {DurationSeconds}s",
            lockResource,
            duration.TotalSeconds);

        IRedLock redLock = await _redLockFactory.CreateLockAsync(
            lockResource,
            duration,
            TimeSpan.FromSeconds(10),
            TimeSpan.FromMilliseconds(200),
            ct);

        if (!redLock.IsAcquired)
        {
            _logger.LogWarning("Failed to acquire distributed lock for resource {Resource}", lockResource);
            return null;
        }

        _logger.LogInformation("Distributed lock acquired for resource {Resource}", lockResource);
        return new RedisDistributedLock(redLock);
    }

    public async Task<bool> PingAsync(CancellationToken ct = default)
    {
        try
        {
            TimeSpan latency = await _db.PingAsync();
            _logger.LogDebug("Redis ping successful in {LatencyMs} ms", latency.TotalMilliseconds);
            return latency.TotalMilliseconds < 5000;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis ping failed");
            return false;
        }
    }

    public ValueTask DisposeAsync()
    {
        _redLockFactory.Dispose();
        return ValueTask.CompletedTask;
    }
}
