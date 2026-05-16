using TaskFlow.Interfaces;
using TaskFlow.Services;

namespace TaskFlow.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
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
        return services;
    }
}