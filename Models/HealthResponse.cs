namespace TaskFlow.Models;

public record HealthResponse
{
    public string Status { get; init; } = "";
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string Service { get; init; } = "";
}