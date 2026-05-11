using TaskFlow.DTOs.Tasks;

namespace TaskFlow.DTOs.Projects;

public record ProjectDetailResponse(
    Guid Id,
    string Name,
    string? Description,
    DateTime CreatedAtUtc,
    IReadOnlyList<TaskSummaryResponse> Tasks
);
