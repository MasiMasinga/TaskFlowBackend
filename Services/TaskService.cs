using Microsoft.EntityFrameworkCore;
using TaskFlow.Data;
using TaskFlow.Enum;
using TaskFlow.Interfaces;
using TaskFlow.Models;

namespace TaskFlow.Services;

public class TaskService : ITaskService
{
    private readonly AppDbContext _db;
    private readonly IClockService _clockService;

    public TaskService(AppDbContext db, IClockService clockService)
    {
        _db = db;
        _clockService = clockService;
    }

    public async Task<List<TaskItem>> GetAllForProjectAsync(Guid projectId, CancellationToken ct)
    {
        return await _db.Tasks
            .AsNoTracking()
            .Where(t => t.ProjectId == projectId)
            .OrderByDescending(t => t.CreatedAtUtc)
            .ToListAsync(ct);
    }

    public async Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _db.Tasks
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, ct);
    }

    public async Task<TaskItem> CreateAsync(Guid projectId, string title, string? description, DateTime? dueDateUtc, CancellationToken ct)
    {
        var projectExists = await _db.Projects.AnyAsync(p => p.Id == projectId, ct);
        if (!projectExists) throw new InvalidOperationException("Project not found");

        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = description,
            DueDateUtc = dueDateUtc,
            Status = TaskItemStatus.Open,
            CreatedAtUtc = _clockService.GetClock().UtcNow,
            ProjectId = projectId,
        };

        _db.Tasks.Add(task);
        await _db.SaveChangesAsync(ct);
        return task;
    }

    public async Task<bool> UpdateAsync(Guid id, string title, string? description, TaskItemStatus status, DateTime? dueDateUtc, CancellationToken ct)
    {
        var task = await _db.Tasks.FirstOrDefaultAsync(t => t.Id == id, ct);
        if (task is null) return false;

        task.Title = title;
        task.Description = description;
        task.Status = status;
        task.DueDateUtc = dueDateUtc;

        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct)
    {
        var task = await _db.Tasks.FirstOrDefaultAsync(t => t.Id == id, ct);
        if (task is null) return false;

        _db.Tasks.Remove(task);
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
