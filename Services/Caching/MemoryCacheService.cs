using Microsoft.Extensions.Caching.Memory;
using TaskFlow.Interfaces;

namespace TaskFlow.Services.Caching;

public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<MemoryCacheService> _logger;

    public MemoryCacheService(IMemoryCache cache, ILogger<MemoryCacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public Task<T?> GetAsync<T>(string key, CancellationToken ct) where T : class
    {
        var hit = _cache.TryGetValue(key, out T? value);
        _logger.LogDebug("Cache {Result} for key {Key}", hit ? "HIT" : "MISS", key);
        return Task.FromResult(value);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken ct) where T : class
    {
        _cache.Set(key, value, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ttl
        });
        _logger.LogDebug("Cache SET for key {Key} (ttl {Ttl})", key, ttl);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken ct)
    {
        _cache.Remove(key);
        _logger.LogDebug("Cache REMOVE for key {Key}", key);
        return Task.CompletedTask;
    }
}