using StackExchange.Redis;
using TaskFlow.Interfaces;

namespace TaskFlow.Services.Caching;


public class RedisAdminCacheService : IAdminCacheService
{
    private readonly IConnectionMultiplexer _multiplexer;
    private readonly ILogger<RedisAdminCacheService> _logger;

    public RedisAdminCacheService(IConnectionMultiplexer multiplexer, ILogger<RedisAdminCacheService> logger)
    {
        _multiplexer = multiplexer;
        _logger = logger;
    }

    public Task<int> ClearForUserAsync(Guid userId, CancellationToken ct)
    {
        var pattern = CacheKeys.RedisPatternForUser(userId);
        var database = _multiplexer.GetDatabase();
        var removed = 0;

        foreach (var endpoint in _multiplexer.GetEndPoints())
        {
            ct.ThrowIfCancellationRequested();

            var server = _multiplexer.GetServer(endpoint);
            if (!server.IsConnected)
                continue;

            var keys = server.Keys(pattern: pattern).ToArray();
            if (keys.Length == 0)
                continue;

            removed += (int)database.KeyDelete(keys);
        }

        _logger.LogInformation(
            "Admin cache clear for user {UserId}: removed {Count} key(s) matching {Pattern}",
            userId,
            removed,
            pattern);

        return Task.FromResult(removed);
    }
}
