using TaskFlow.Enum;
using TaskFlow.Models;

namespace TaskFlow.Interfaces;

public interface ITaskService
{
    Task<List<TaskItem>> GetAllForProjectAsync(Guid projectId, CancellationToken ct);
    Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<TaskItem?> CreateAsync(Guid projectId, string title, string? description, DateTime? dueDateUtc, CancellationToken ct);
    Task<bool> UpdateAsync(Guid id, string title, string? description, TaskItemStatus status, DateTime? dueDateUtc, CancellationToken ct);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct);
}