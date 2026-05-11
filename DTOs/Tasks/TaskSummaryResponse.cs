using TaskFlow.Enum;

namespace TaskFlow.DTOs.Tasks;

public record TaskSummaryResponse(
    Guid Id,
    string Title,
    TaskItemStatus Status,
    DateTime? DueDateUtc
);
