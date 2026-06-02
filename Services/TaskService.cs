using Microsoft.EntityFrameworkCore;
using TaskFlow.Data;
using TaskFlow.Enum;
using TaskFlow.Interfaces;
using TaskFlow.Models;
using TaskFlow.Services.Caching;
using TaskFlow.DTOs.Tasks;
using TaskFlow.Models.Pagination;

namespace TaskFlow.Services;

public class TaskService : ITaskService
{
    private static readonly TimeSpan TasksCacheTtl = TimeSpan.FromMinutes(2);

    private readonly AppDbContext _db;
    private readonly IClockService _clockService;
    private readonly ICacheService _cache;

    public TaskService(AppDbContext db, IClockService clockService, ICacheService cache)
    {
        _db = db;
        _clockService = clockService;
        _cache = cache;
    }

    public async Task<PagedResult<TaskItem>?> GetForProjectAsync(
    Guid projectId,
    Guid userId,
    TaskListRequest request,
    CancellationToken ct)
    {
        var projectExists = await _db.Projects
            .AnyAsync(p => p.Id == projectId && p.OwnerId == userId, ct);

        if (!projectExists) return null;

        var query = _db.Tasks
            .AsNoTracking()
            .Where(t => t.ProjectId == projectId);

        if (request.Status.HasValue)
            query = query.Where(t => t.Status == request.Status.Value);

        if (request.DueBeforeUtc.HasValue)
            query = query.Where(t => t.DueDateUtc != null && t.DueDateUtc < request.DueBeforeUtc.Value);

        if (request.DueAfterUtc.HasValue)
            query = query.Where(t => t.DueDateUtc != null && t.DueDateUtc > request.DueAfterUtc.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var pattern = $"%{request.Search.Trim()}%";
            query = query.Where(t => EF.Functions.ILike(t.Title, pattern));
        }

        query = ApplySort(query, request.Sort);

        return await query.ToPagedResultAsync(request.Page, request.PageSize, ct);
    }

    public async Task<List<TaskItem>> GetAllForProjectAsync(
        Guid projectId,
        Guid userId,
        CancellationToken ct)
    {
        var key = CacheKeys.TasksForProject(projectId, userId);

        var cached = await _cache.GetAsync<List<TaskItem>>(key, ct);
        if (cached is not null) return cached;

        var tasks = await _db.Tasks
            .AsNoTracking()
            .Where(t => t.ProjectId == projectId && t.Project.OwnerId == userId)
            .OrderByDescending(t => t.CreatedAtUtc)
            .ToListAsync(ct);

        await _cache.SetAsync(key, tasks, TasksCacheTtl, ct);
        return tasks;
    }

    public async Task<TaskItem?> GetByIdAsync(Guid id, Guid userId, CancellationToken ct)
    {

        var key = CacheKeys.TaskDetail(id, userId);

        var cached = await _cache.GetAsync<TaskItem>(key, ct);
        if (cached is not null) return cached;

        var task = await _db.Tasks
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id && t.Project.OwnerId == userId, ct);

        if (task is not null)
            await _cache.SetAsync(key, task, TasksCacheTtl, ct);

        return task;
    }

    public async Task<TaskItem> CreateAsync(Guid projectId, Guid userId, string title, string? description, DateTime? dueDateUtc, CancellationToken ct)
    {
        var projectExists = await _db.Projects.AnyAsync(p => p.Id == projectId && p.OwnerId == userId, ct);
        if (!projectExists) throw new InvalidOperationException("Project not found");

        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = description,
            DueDateUtc = dueDateUtc,
            Status = TaskItemStatus.Open,
            CreatedAtUtc = _clockService.GetClock().UtcNow,
        };

        _db.Tasks.Add(task);

        await _db.SaveChangesAsync(ct);
        await _cache.RemoveAsync(CacheKeys.TasksForProject(projectId, userId), ct);

        return task;
    }

    public async Task<bool> UpdateAsync(Guid id, Guid userId, string title, string? description, TaskItemStatus status, DateTime? dueDateUtc, CancellationToken ct)
    {
        var task = await _db.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.Project.OwnerId == userId, ct);
        if (task is null) return false;

        task.Title = title;
        task.Description = description;
        task.Status = status;
        task.DueDateUtc = dueDateUtc;

        await _db.SaveChangesAsync(ct);
        await _cache.RemoveAsync(CacheKeys.TaskDetail(id, userId), ct);
        await _cache.RemoveAsync(CacheKeys.TasksForProject(task.ProjectId, userId), ct);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid userId, CancellationToken ct)
    {
        var task = await _db.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.Project.OwnerId == userId, ct);
        if (task is null) return false;

        _db.Tasks.Remove(task);
        await _db.SaveChangesAsync(ct);
        await _cache.RemoveAsync(CacheKeys.TaskDetail(id, userId), ct);
        await _cache.RemoveAsync(CacheKeys.TasksForProject(task.ProjectId, userId), ct);
        return true;
    }

    public async Task<bool> UpdateStatusAsync(Guid id, Guid userId, TaskItemStatus status, CancellationToken ct)
    {
        var task = await _db.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.Project.OwnerId == userId, ct);
        if (task is null) return false;

        task.Status = status;
        await _db.SaveChangesAsync(ct);
        await _cache.RemoveAsync(CacheKeys.TaskDetail(id, userId), ct);
        await _cache.RemoveAsync(CacheKeys.TasksForProject(task.ProjectId, userId), ct);
        return true;
    }

    private static IQueryable<TaskItem> ApplySort(IQueryable<TaskItem> query, string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort))
            return query.OrderByDescending(t => t.CreatedAtUtc);

        var descending = sort.StartsWith('-');
        var field = descending ? sort[1..] : sort;

        return (field.ToLowerInvariant(), descending) switch
        {
            ("title", false) => query.OrderBy(t => t.Title),
            ("title", true) => query.OrderByDescending(t => t.Title),
            ("status", false) => query.OrderBy(t => t.Status),
            ("status", true) => query.OrderByDescending(t => t.Status),
            ("duedate", false) => query.OrderBy(t => t.DueDateUtc),
            ("duedate", true) => query.OrderByDescending(t => t.DueDateUtc),
            ("createdat", false) => query.OrderBy(t => t.CreatedAtUtc),
            ("createdat", true) => query.OrderByDescending(t => t.CreatedAtUtc),
            _ => query.OrderByDescending(t => t.CreatedAtUtc)
        };
    }
}
