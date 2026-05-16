using Microsoft.EntityFrameworkCore;
using TaskFlow.Data;
using TaskFlow.Enum;
using TaskFlow.Interfaces;
using TaskFlow.Models;

namespace TaskFlow.Services;

public class ProjectService : IProjectService
{
    private readonly AppDbContext _db;
    private readonly IClockService _clockService;

    public ProjectService(AppDbContext db, IClockService clockService)
    {
        _db = db;
        _clockService = clockService;
    }

    public async Task<List<Project>> GetAllAsync(Guid userId, CancellationToken ct)
    {
        return await _db.Projects
            .AsNoTracking()
            .Where(p => p.OwnerId == userId)
            .OrderByDescending(p => p.CreatedAtUtc)
            .ToListAsync(ct);
    }

    public async Task<Project?> GetByIdAsync(Guid id, Guid userId, CancellationToken ct)
    {
        return await _db.Projects
            .AsNoTracking()
            .Include(p => p.Tasks)
            .FirstOrDefaultAsync(p => p.Id == id && p.OwnerId == userId, ct);
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
        return await _db.Projects.AnyAsync(p => p.Id == id && p.OwnerId == userId, ct);
    }
}