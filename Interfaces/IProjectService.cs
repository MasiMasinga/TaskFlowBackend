using TaskFlow.DTOs.Projects;
using TaskFlow.Enum;
using TaskFlow.Models;

namespace TaskFlow.Interfaces;

public interface IProjectService
{
    Task<List<ProjectResponse>> GetAllAsync(Guid userId, CancellationToken ct);
    Task<ProjectDetailResponse?> GetByIdAsync(Guid id, Guid userId, CancellationToken ct);
    Task<ProjectResponse> CreateAsync(Guid userId, string name, string? description, CancellationToken ct);
    Task<Project> UpdateAsync(Guid id, Guid userId, string name, string? description, CancellationToken ct);
    Task<ProjectDeleteResult> DeleteAsync(Guid id, Guid userId, CancellationToken ct, bool force);
    Task<bool> ExistsAsync(Guid id, Guid userId, CancellationToken ct);
}
