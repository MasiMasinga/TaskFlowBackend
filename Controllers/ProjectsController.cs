using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.DTOs.Projects;
using TaskFlow.Enum;
using TaskFlow.Interfaces;

namespace TaskFlow.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projects;
    private readonly ICurrentUser _currentUser;

    public ProjectsController(IProjectService projects, ICurrentUser currentUser)
    {
        _projects = projects;
        _currentUser = currentUser;
    }
    private Guid UserId => _currentUser.UserId ?? throw new InvalidOperationException("Authenticated user has no ID claim.");

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ProjectResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProjectResponse>>> GetAll(CancellationToken ct)
    {
        var projects = await _projects.GetAllAsync(UserId, ct);
        return Ok(projects);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProjectDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProjectDetailResponse>> GetById(Guid id, CancellationToken ct)
    {
        var project = await _projects.GetByIdAsync(id, UserId, ct);
        return project is null ? NotFound() : Ok(project);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ProjectResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProjectResponse>> Create(
        [FromBody] CreateProjectRequest request,
        CancellationToken ct)
    {
        var response = await _projects.CreateAsync(UserId, request.Name, request.Description, ct);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
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
        var project = await _projects.UpdateAsync(id, UserId, request.Name, request.Description, ct);
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
        var result = await _projects.DeleteAsync(id, UserId, ct, force);

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
        var project = await _projects.ExistsAsync(id, UserId, ct);
        return project ? NoContent() : NotFound();
    }
}
