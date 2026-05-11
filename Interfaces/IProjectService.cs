using TaskFlow.Models;

namespace TaskFlow.Interfaces;

public interface IProjectService
{
    Task<List<Project>> GetAllAsync(CancellationToken ct);
    Task<Project?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<Project> CreateAsync(string name, string? description, CancellationToken ct);
    Task<Project> UpdateAsync(Guid id, string name, string? description, CancellationToken ct);
}
