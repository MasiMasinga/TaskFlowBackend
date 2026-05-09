namespace TaskFlow.Models;

public record HealthDetails
(
    TimeSpan ProcessTime,
    string MachineName,
    string CurrentEnvironment
);