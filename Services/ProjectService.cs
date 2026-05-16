using Microsoft.EntityFrameworkCore;
using TaskFlow.Data;
using TaskFlow.Interfaces;
using TaskFlow.Models;
using TaskFlow.Enum;

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

    public async Task<List<Project>> GetAllAsync(CancellationToken ct)
    {
        return await _db.Projects
            .AsNoTracking()
            .OrderByDescending(p => p.CreatedAtUtc)
            .ToListAsync(ct);
    }

    public async Task<Project?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _db.Projects
            .AsNoTracking()
            .Include(p => p.Tasks)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<Project> CreateAsync(string name, string? description, CancellationToken ct)
    {
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            CreatedAtUtc = _clockService.GetClock().UtcNow
        };

        _db.Projects.Add(project);
        await _db.SaveChangesAsync(ct);
        return project;
    }

    public async Task<Project> UpdateAsync(Guid id, string name, string? description, CancellationToken ct)
    {
        var project = await _db.Projects.FindAsync(id, ct);

        if (project is null)
            throw new InvalidOperationException("Project not found");

        project.Name = name;
        project.Description = description;

        await _db.SaveChangesAsync(ct);
        return project;
    }

    public async Task<ProjectDeleteResult> DeleteAsync(Guid id, CancellationToken ct, bool force)
    {
        if (!force)
        {
            var hasTasks = await _db.Tasks.AnyAsync(t => t.ProjectId == id, ct);
            if (hasTasks) return ProjectDeleteResult.HasTasks;
        }

        var project = await _db.Projects.FindAsync(id, ct);
        if (project is null) return ProjectDeleteResult.NotFound;

        _db.Projects.Remove(project);
        await _db.SaveChangesAsync(ct);
        return ProjectDeleteResult.Deleted;
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct)
    {
        return await _db.Projects.AnyAsync(p => p.Id == id, ct);
    }
}