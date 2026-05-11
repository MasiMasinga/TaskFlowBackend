using TaskFlow.DTOs.Tasks;
using TaskFlow.Models;

namespace TaskFlow.Mappings;

public static class TaskMappings
{
    public static TaskResponse ToResponse(this TaskItem task)
    {
        return new TaskResponse(
            Id: task.Id,
            ProjectId: task.ProjectId,
            Title: task.Title,
            Description: task.Description,
            Status: task.Status,
            DueDateUtc: task.DueDateUtc,
            CreatedAtUtc: task.CreatedAtUtc
        );
    }

    public static TaskSummaryResponse ToSummaryResponse(this TaskItem task)
    {
        return new TaskSummaryResponse(
            Id: task.Id,
            Title: task.Title,
            Status: task.Status,
            DueDateUtc: task.DueDateUtc
        );
    }
}