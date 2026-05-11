using Microsoft.EntityFrameworkCore;
using TaskFlow.Data;
using TaskFlow.Interfaces;
using TaskFlow.Models;

namespace TaskFlow.Api.Services;

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
}