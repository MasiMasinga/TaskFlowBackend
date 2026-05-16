using Microsoft.EntityFrameworkCore;
using TaskFlow.Data;
using TaskFlow.Enum;
using TaskFlow.Interfaces;
using TaskFlow.Models;
using TaskFlow.Services.Caching;

namespace TaskFlow.Services;

public class ProjectService : IProjectService
{
    private static readonly TimeSpan ProjectsCacheTtl = TimeSpan.FromMinutes(2);

    private readonly AppDbContext _db;
    private readonly IClockService _clockService;
    private readonly ICacheService _cache;

    public ProjectService(AppDbContext db, IClockService clockService, ICacheService cache)
    {
        _db = db;
        _clockService = clockService;
        _cache = cache;
    }

    public async Task<List<Project>> GetAllAsync(Guid userId, CancellationToken ct)
    {
        var key = CacheKeys.ProjectsForUser(userId);

        var cached = await _cache.GetAsync<List<Project>>(key, ct);
        if (cached is not null) return cached;

        var projects = await _db.Projects
            .AsNoTracking()
            .Where(p => p.OwnerId == userId)
            .OrderByDescending(p => p.CreatedAtUtc)
            .ToListAsync(ct);

        await _cache.SetAsync(key, projects, ProjectsCacheTtl, ct);
        return projects;
    }

    public async Task<Project?> GetByIdAsync(Guid id, Guid userId, CancellationToken ct)
    {
        var key = CacheKeys.ProjectDetail(id, userId);

        var cached = await _cache.GetAsync<Project>(key, ct);
        if (cached is not null) return cached;

        var project = await _db.Projects
            .AsNoTracking()
            .Include(p => p.Tasks)
            .FirstOrDefaultAsync(p => p.Id == id && p.OwnerId == userId, ct);

        if (project is not null)
            await _cache.SetAsync(key, project, ProjectsCacheTtl, ct);

        return project;
    }

    public async Task<Project> CreateAsync(Guid userId, string name, string? description, CancellationToken ct)
    {
        var project = new Project
        {
            Id = Guid.NewGuid(),
            OwnerId = userId,
            Name = name,
            Description = description,
            CreatedAtUtc = _clockService.GetClock().UtcNow
        };

        _db.Projects.Add(project);

        await _db.SaveChangesAsync(ct);
        await _cache.RemoveAsync(CacheKeys.ProjectsForUser(userId), ct);

        return project;
    }

    public async Task<Project> UpdateAsync(Guid id, Guid userId, string name, string? description, CancellationToken ct)
    {
        var project = await _db.Projects
            .FirstOrDefaultAsync(p => p.Id == id && p.OwnerId == userId, ct);

        if (project is null)
            throw new InvalidOperationException("Project not found");

        project.Name = name;
        project.Description = description;

        await _db.SaveChangesAsync(ct);
        await _cache.RemoveAsync(CacheKeys.ProjectsForUser(userId), ct);
        await _cache.RemoveAsync(CacheKeys.ProjectDetail(id, userId), ct);

        return project;
    }

    public async Task<ProjectDeleteResult> DeleteAsync(Guid id, Guid userId, CancellationToken ct, bool force)
    {
        if (!force)
        {
            var hasTasks = await _db.Tasks.AnyAsync(t => t.ProjectId == id && t.Project.OwnerId == userId, ct);
            if (hasTasks) return ProjectDeleteResult.HasTasks;
        }

        var project = await _db.Projects.FirstOrDefaultAsync(p => p.Id == id && p.OwnerId == userId, ct);
        if (project is null) return ProjectDeleteResult.NotFound;

        _db.Projects.Remove(project);
        await _db.SaveChangesAsync(ct);
        return ProjectDeleteResult.Deleted;
    }

    public async Task<bool> ExistsAsync(Guid id, Guid userId, CancellationToken ct)
    {
        var project = await _db.Projects.FirstOrDefaultAsync(p => p.Id == id && p.OwnerId == userId, ct);
        if (project is null) return false;

        _db.Projects.Remove(project);
        await _db.SaveChangesAsync(ct);

        await _cache.RemoveAsync(CacheKeys.ProjectsForUser(userId), ct);
        await _cache.RemoveAsync(CacheKeys.ProjectDetail(id, userId), ct);
        return true;
    }
}