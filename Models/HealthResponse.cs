namespace TaskFlow.Models;

public record HealthResponse
(
    string Status,
    DateTime Timestamp,
    string Service
);