using TaskFlow.Enum;
using TaskFlow.Models;
using TaskFlow.DTOs.Tasks;
using TaskFlow.Models.Pagination;

namespace TaskFlow.Interfaces;

public interface ITaskService
{
    Task<PagedResult<TaskItem>?> GetForProjectAsync(Guid projectId, Guid userId, TaskListRequest request, CancellationToken ct);
    Task<List<TaskItem>> GetAllForProjectAsync(Guid projectId, Guid userId, CancellationToken ct);
    Task<TaskItem?> GetByIdAsync(Guid id, Guid userId, CancellationToken ct);
    Task<TaskItem?> CreateAsync(Guid projectId, Guid userId, string title, string? description, DateTime? dueDateUtc, CancellationToken ct);
    Task<bool> UpdateAsync(Guid id, Guid userId, string title, string? description, TaskItemStatus status, DateTime? dueDateUtc, CancellationToken ct);
    Task<bool> DeleteAsync(Guid id, Guid userId, CancellationToken ct);
    Task<bool> UpdateStatusAsync(Guid id, Guid userId, TaskItemStatus status, CancellationToken ct);
}