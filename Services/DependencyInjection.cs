using TaskFlow.Interfaces;

namespace TaskFlow.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddSingleton<IClockService, ClockService>();
        services.AddSingleton<IGreetingService, GreetingsService>();
        services.AddSingleton<IHealthService, HealthService>();
        services.AddSingleton<ISystemInfoService, SystemInfoService>();
        services.AddSingleton<IVersionService, VersionService>();
        return services;
    }
}