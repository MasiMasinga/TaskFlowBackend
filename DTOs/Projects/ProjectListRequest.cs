using TaskFlow.Models.Pagination;

namespace TaskFlow.DTOs.Projects;

public class ProjectListRequest : PagedRequest
{
    public string? Search { get; set; }
    public string? Sort { get; set; }  // "name", "-createdAt", etc.
}