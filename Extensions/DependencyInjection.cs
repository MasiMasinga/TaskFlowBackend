using TaskFlow.Interfaces;
using TaskFlow.Services;
using TaskFlow.Api.Services;

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
        return services;
    }
}