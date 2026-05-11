using TaskFlow.DTOs.Projects;
using TaskFlow.DTOs.Tasks;
using TaskFlow.Models;

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
}