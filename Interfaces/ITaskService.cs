using TaskFlow.Models;
using TaskFlow.Enum;

namespace TaskFlow.Interfaces;

public interface ITaskService
{
    Task<List<TaskItem>> GetAllAsync(CancellationToken ct);
    Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<TaskItem> CreateAsync(string title, string? description, DateTime? dueDateUtc, Priority priority, Guid projectId, CancellationToken ct);
}
