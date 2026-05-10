using Microsoft.AspNetCore.Mvc;
using TaskFlow.Enum;
using TaskFlow.Interfaces;

namespace TaskFlow.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _tasks;

    public TasksController(ITaskService tasks)
    {
        _tasks = tasks;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var tasks = await _tasks.GetAllAsync(ct);
        return Ok(tasks);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var task = await _tasks.GetByIdAsync(id, ct);
        return task is null ? NotFound() : Ok(task);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTaskRequest request, CancellationToken ct)
    {
        var task = await _tasks.CreateAsync(request.Title, request.Description, request.DueDateUtc, request.Priority, request.ProjectId, ct);
        return CreatedAtAction(nameof(GetById), new { id = task.Id }, task);
    }
}

public record CreateTaskRequest(string Title, string? Description, DateTime? DueDateUtc, Priority Priority, Guid ProjectId);