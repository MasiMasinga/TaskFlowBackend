namespace TaskFlow.Models;

public record HealthDetails
{
    public TimeSpan ProcessTime { get; init; } = TimeSpan.Zero;
    public string MachineName { get; init; } = "";
    public string CurrentEnvironment { get; init; } = "";
}