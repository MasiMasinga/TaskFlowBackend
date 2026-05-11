using Microsoft.AspNetCore.Mvc;
using TaskFlow.DTOs.Projects;
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
    public async Task<ActionResult<IReadOnlyList<ProjectResponse>>> GetAll(CancellationToken ct)
    {
        var projects = await _projects.GetAllAsync(ct);
        var response = projects.Select(p => p.ToResponse()).ToList();
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProjectDetailResponse>> GetById(Guid id, CancellationToken ct)
    {
        var project = await _projects.GetByIdAsync(id, ct);
        return project is null ? NotFound() : Ok(project.ToDetailResponse());
    }

    [HttpPost]
    public async Task<ActionResult<ProjectResponse>> Create(
        [FromBody] CreateProjectRequest request,
        CancellationToken ct)
    {
        var project = await _projects.CreateAsync(request.Name, request.Description, ct);
        var response = project.ToResponse();
        return CreatedAtAction(nameof(GetById), new { id = project.Id }, response);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProjectResponse>> Update(
        Guid id,
        [FromBody] UpdateProjectRequest request,
        CancellationToken ct)
    {
        var project = await _projects.UpdateAsync(id, request.Name, request.Description, ct);
        var response = project.ToResponse();
        return Ok(response);
    }
}