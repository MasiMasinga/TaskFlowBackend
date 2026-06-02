using TaskFlow.DTOs.Tasks;
using TaskFlow.Models;
using TaskFlow.Models.Pagination;

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

    public static PagedResult<TaskResponse> ToResponse(this PagedResult<TaskItem> page)
    {
        return new PagedResult<TaskResponse>(
            Items: page.Items.Select(t => t.ToResponse()).ToList(),
            Page: page.Page,
            PageSize: page.PageSize,
            TotalCount: page.TotalCount);
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