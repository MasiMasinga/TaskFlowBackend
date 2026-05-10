using Microsoft.AspNetCore.Mvc;
using TaskFlow.Interfaces;

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
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var projects = await _projects.GetAllAsync(ct);
        return Ok(projects);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var project = await _projects.GetByIdAsync(id, ct);
        return project is null ? NotFound() : Ok(project);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProjectRequest request, CancellationToken ct)
    {
        var project = await _projects.CreateAsync(request.Name, request.Description, ct);
        return CreatedAtAction(nameof(GetById), new { id = project.Id }, project);
    }
}

public record CreateProjectRequest(string Name, string? Description);