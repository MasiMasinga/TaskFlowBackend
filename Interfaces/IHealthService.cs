using TaskFlow.Models;
namespace TaskFlow.Interfaces;

public interface IHealthService
{
    HealthResponse GetHealth();
    HealthDetails GetHealthDetails();
}
