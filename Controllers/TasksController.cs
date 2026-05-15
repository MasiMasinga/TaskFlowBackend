using Microsoft.AspNetCore.Mvc;
using TaskFlow.DTOs.Tasks;
using TaskFlow.Interfaces;
using TaskFlow.Mappings;

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

    [HttpGet("/projects/{projectId:guid}/tasks")]
    [ProducesResponseType(typeof(IReadOnlyList<TaskResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TaskResponse>>> GetAllForProject(
        Guid projectId,
        CancellationToken ct)
    {
        var tasks = await _tasks.GetAllForProjectAsync(projectId, ct);
        var response = tasks.Select(t => t.ToResponse()).ToList();
        return Ok(response);
    }

    [HttpGet("/tasks/{id:guid}", Name = nameof(GetTaskById))]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskResponse>> GetTaskById(Guid id, CancellationToken ct)
    {
        var task = await _tasks.GetByIdAsync(id, ct);
        return task is null ? NotFound() : Ok(task.ToResponse());
    }

    [HttpPost("/projects/{projectId:guid}/tasks")]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskResponse>> Create(
        Guid projectId,
        [FromBody] CreateTaskRequest request,
        CancellationToken ct)
    {
        var task = await _tasks.CreateAsync(
            projectId, request.Title, request.Description, request.DueDateUtc, ct);

        if (task is null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Project not found",
                Detail = $"No project exists with id {projectId}."
            });
        }

        var response = task.ToResponse();
        return CreatedAtRoute(nameof(GetTaskById), new { id = task.Id }, response);
    }

    [HttpPut("/tasks/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateTaskRequest request,
        CancellationToken ct)
    {
        var found = await _tasks.UpdateAsync(
            id, request.Title, request.Description, request.Status, request.DueDateUtc, ct);

        return found ? NoContent() : NotFound();
    }

    [HttpDelete("/tasks/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var found = await _tasks.DeleteAsync(id, ct);
        return found ? NoContent() : NotFound();
    }
}