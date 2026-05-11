namespace TaskFlow.DTOs.Tasks;

public record CreateTaskRequest(
    string Title,
    string? Description,
    DateTime? DueDateUtc
);