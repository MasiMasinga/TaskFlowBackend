using TaskFlow.Enum;

namespace TaskFlow.DTOs.Tasks;

public record UpdateTaskRequest(
    string Title,
    string? Description,
    TaskItemStatus Status,
    DateTime? DueDateUtc
);
