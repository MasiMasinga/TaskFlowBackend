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

    public async Task<List<TaskItem>> GetAllAsync(CancellationToken ct)
    {
        return await _db.Tasks
            .AsNoTracking()
            .OrderByDescending(t => t.CreatedAtUtc)
            .ToListAsync(ct);
    }

    public async Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _db.Tasks
            .AsNoTracking()
            .Include(t => t.Project)
            .FirstOrDefaultAsync(t => t.Id == id, ct);
    }

    public async Task<TaskItem> CreateAsync(string title, string? description, DateTime? dueDateUtc, Priority priority, Guid projectId, CancellationToken ct)
    {
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = description,
            DueDateUtc = dueDateUtc,
            Status = TaskItemStatus.Open,
            Priority = priority,
            CreatedAtUtc = _clockService.GetClock().UtcNow,
            ProjectId = projectId,
        };

        if (projectId == Guid.Empty)
            throw new InvalidOperationException("Project ID is required");

        _db.Tasks.Add(task);

        await _db.SaveChangesAsync(ct);
        return task;
    }
}
