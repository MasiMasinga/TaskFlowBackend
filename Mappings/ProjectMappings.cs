using TaskFlow.DTOs.Projects;
using TaskFlow.Models;
using TaskFlow.Models.Pagination;

namespace TaskFlow.Mappings;

public static class ProjectMappings
{
    public static ProjectResponse ToResponse(this Project project)
    {
        return new ProjectResponse(
            Id: project.Id,
            Name: project.Name,
            Description: project.Description,
            CreatedAtUtc: project.CreatedAtUtc
        );
    }

    public static ProjectDetailResponse ToDetailResponse(this Project project)
    {
        return new ProjectDetailResponse(
            Id: project.Id,
            Name: project.Name,
            Description: project.Description,
            CreatedAtUtc: project.CreatedAtUtc,
            Tasks: project.Tasks
                .Select(t => t.ToSummaryResponse())
                .ToList()
        );
    }

    public static PagedResult<ProjectResponse> ToResponse(this PagedResult<Project> page)
    {
        return new PagedResult<ProjectResponse>(
            Items: page.Items.Select(p => p.ToResponse()).ToList(),
            Page: page.Page,
            PageSize: page.PageSize,
            TotalCount: page.TotalCount);
    }
}