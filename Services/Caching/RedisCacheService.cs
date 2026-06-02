using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using TaskFlow.Interfaces;

namespace TaskFlow.Services.Caching;

public class RedisCacheService : ICacheService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly IDistributedCache _cache;
    private readonly ILogger<RedisCacheService> _logger;

    public RedisCacheService(IDistributedCache cache, ILogger<RedisCacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct) where T : class
    {
        var bytes = await _cache.GetAsync(key, ct);
        if (bytes is null)
        {
            _logger.LogDebug("Cache MISS for key {Key}", key);
            return null;
        }

        _logger.LogDebug("Cache HIT for key {Key}", key);
        return JsonSerializer.Deserialize<T>(bytes, JsonOptions);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken ct) where T : class
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(value, JsonOptions);
        await _cache.SetAsync(key, bytes, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ttl
        }, ct);
        _logger.LogDebug("Cache SET for key {Key} (ttl {Ttl})", key, ttl);
    }

    public async Task RemoveAsync(string key, CancellationToken ct)
    {
        await _cache.RemoveAsync(key, ct);
        _logger.LogDebug("Cache REMOVE for key {Key}", key);
    }
}