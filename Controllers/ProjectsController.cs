using Microsoft.AspNetCore.Mvc;
using TaskFlow.DTOs.Projects;
using TaskFlow.Enum;
using TaskFlow.Interfaces;
using TaskFlow.Mappings;

namespace TaskFlow.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projects;

    public ProjectsController(IProjectService projects)
    {
        _projects = projects;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ProjectResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProjectResponse>>> GetAll(CancellationToken ct)
    {
        var projects = await _projects.GetAllAsync(ct);
        return Ok(projects.Select(p => p.ToResponse()).ToList());
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProjectDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProjectDetailResponse>> GetById(Guid id, CancellationToken ct)
    {
        var project = await _projects.GetByIdAsync(id, ct);
        return project is null ? NotFound() : Ok(project.ToDetailResponse());
    }

    [HttpPost]
    [ProducesResponseType(typeof(ProjectResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProjectResponse>> Create(
        [FromBody] CreateProjectRequest request,
        CancellationToken ct)
    {
        var project = await _projects.CreateAsync(request.Name, request.Description, ct);
        var response = project.ToResponse();
        return CreatedAtAction(nameof(GetById), new { id = project.Id }, response);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateProjectRequest request,
        CancellationToken ct)
    {
        var project = await _projects.UpdateAsync(id, request.Name, request.Description, ct);
        return project is null ? NotFound() : NoContent();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken ct,
        [FromQuery(Name = "force")] bool force = false)
    {
        var result = await _projects.DeleteAsync(id, ct, force);

        return result switch
        {
            ProjectDeleteResult.Deleted => NoContent(),
            ProjectDeleteResult.NotFound => NotFound(),
            ProjectDeleteResult.HasTasks => Problem(
                title: "Cannot delete project",
                detail: "This project has one or more tasks. Retry the request with force=true to delete the project and all associated tasks.",
                statusCode: StatusCodes.Status409Conflict),
            _ => throw new ArgumentOutOfRangeException(nameof(result), result, null)
        };
    }

    [HttpGet("/api/projects/{id:guid}/exists")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Exists(Guid id, CancellationToken ct)
    {
        var project = await _projects.ExistsAsync(id, ct);
        return project ? NoContent() : NotFound();
    }
}