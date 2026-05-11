using TaskFlow.Enum;

namespace TaskFlow.DTOs.Tasks;

public record TaskResponse(
    Guid Id,
    Guid ProjectId,
    string Title,
    string? Description,
    TaskItemStatus Status,
    DateTime? DueDateUtc,
    DateTime CreatedAtUtc
);
