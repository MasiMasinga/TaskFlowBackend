namespace TaskFlow.DTOs.Projects;

public record ProjectResponse(
    Guid Id,
    string Name,
    string? Description,
    DateTime CreatedAtUtc
);
