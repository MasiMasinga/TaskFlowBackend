using TaskFlow.Enum;
namespace TaskFlow.DTOs.Tasks;

public record CreateTaskRequest(
    string Title,
    string? Description,
    Priority Priority,
    DateTime? DueDateUtc,
    Guid ProjectId
);