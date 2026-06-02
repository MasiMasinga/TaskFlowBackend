using TaskFlow.DTOs.Projects;
using TaskFlow.Enum;
using TaskFlow.Models;
using TaskFlow.Models.Pagination;

namespace TaskFlow.Interfaces;

public interface IProjectService
{
    Task<PagedResult<Project>> GetForUserAsync(Guid userId, ProjectListRequest request, CancellationToken ct);
    Task<List<ProjectResponse>> GetAllAsync(Guid userId, ProjectListRequest request, CancellationToken ct);
    Task<ProjectDetailResponse?> GetByIdAsync(Guid id, Guid userId, CancellationToken ct);
    Task<ProjectResponse> CreateAsync(Guid userId, string name, string? description, CancellationToken ct);
    Task<Project> UpdateAsync(Guid id, Guid userId, string name, string? description, CancellationToken ct);
    Task<ProjectDeleteResult> DeleteAsync(Guid id, Guid userId, CancellationToken ct, bool force);
    Task<bool> ExistsAsync(Guid id, Guid userId, CancellationToken ct);
}
