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
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis");
            options.InstanceName = "taskflow:";
        });
        services.AddSingleton<ICacheService, RedisCacheService>();
        return services;
    }
}