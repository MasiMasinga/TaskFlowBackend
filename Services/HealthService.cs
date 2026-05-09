using TaskFlow.Interfaces;
using TaskFlow.Models;
namespace TaskFlow.Services;

public class HealthService : IHealthService
{
    public HealthResponse GetHealth()
    {
        return new HealthResponse(
            Status: "Healthy",
            Timestamp: DateTime.UtcNow,
            Service: "TaskFlow.Api"
        );
    }

    public HealthDetails GetHealthDetails()
    {
        return new HealthDetails(
            ProcessTime: TimeSpan.FromSeconds(1),
            MachineName: Environment.MachineName,
            CurrentEnvironment: Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development" ?? "Production"
        );
    }
}
