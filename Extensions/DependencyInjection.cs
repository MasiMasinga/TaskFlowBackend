using StackExchange.Redis;
using TaskFlow.Interfaces;
using TaskFlow.Services;
using TaskFlow.Services.Caching;

namespace TaskFlow.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<IClockService, ClockService>();
        services.AddSingleton<IGreetingService, GreetingsService>();
        services.AddSingleton<IHealthService, HealthService>();
        services.AddSingleton<ISystemInfoService, SystemInfoService>();
        services.AddSingleton<IVersionService, VersionService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<ITaskService, TaskService>();
        services.AddSingleton<ITokenService, TokenService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddMemoryCache();
        // services.AddSingleton<ICacheService, MemoryCacheService>();
        var redisConnection = configuration.GetConnectionString("Redis")
            ?? throw new InvalidOperationException("Connection string 'Redis' is not configured.");

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnection;
            options.InstanceName = CacheKeys.InstanceName;
        });
        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnection));
        services.AddSingleton<ICacheService, RedisCacheService>();
        services.AddSingleton<IAdminCacheService, RedisAdminCacheService>();
        return services;
    }
}