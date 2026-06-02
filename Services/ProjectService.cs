using Microsoft.EntityFrameworkCore;
using TaskFlow.Data;
using TaskFlow.DTOs.Projects;
using TaskFlow.Enum;
using TaskFlow.Interfaces;
using TaskFlow.Mappings;
using TaskFlow.Models;
using TaskFlow.Services.Caching;
using TaskFlow.Models.Pagination;
using TaskFlow.Exceptions;

namespace TaskFlow.Services;

public class ProjectService : IProjectService
{
    private static readonly TimeSpan ProjectsCacheTtl = TimeSpan.FromMinutes(2);

    private readonly AppDbContext _db;
    private readonly IClockService _clockService;
    private readonly ICacheService _cache;
    private readonly ILogger<ProjectService> _logger;

    public ProjectService(AppDbContext db, IClockService clockService, ICacheService cache, ILogger<ProjectService> logger)
    {
        _db = db;
        _clockService = clockService;
        _cache = cache;
        _logger = logger;
    }

    public async Task<PagedResult<Project>> GetForUserAsync(Guid userId, ProjectListRequest request, CancellationToken ct)
    {
        var query = _db.Projects
            .AsNoTracking()
            .Where(p => p.OwnerId == userId);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var pattern = $"%{request.Search.Trim()}%";
            query = query.Where(p => EF.Functions.ILike(p.Name, pattern));
        }

        query = ApplySort(query, request.Sort);

        return await query.ToPagedResultAsync(request.Page, request.PageSize, ct);
    }

    public async Task<List<ProjectResponse>> GetAllAsync(Guid userId, CancellationToken ct)
    {
        var key = CacheKeys.ProjectsForUser(userId);

        var cached = await _cache.GetAsync<List<ProjectResponse>>(key, ct);
        if (cached is not null) return cached;

        var projects = await _db.Projects
            .AsNoTracking()
            .Where(p => p.OwnerId == userId)
            .OrderByDescending(p => p.CreatedAtUtc)
            .ToListAsync(ct);

        var responses = projects.Select(p => p.ToResponse()).ToList();
        await _cache.SetAsync(key, responses, ProjectsCacheTtl, ct);
        return responses;
    }

    public async Task<ProjectDetailResponse?> GetByIdAsync(Guid id, Guid userId, CancellationToken ct)
    {
        var key = CacheKeys.ProjectDetail(id, userId);

        var cached = await _cache.GetAsync<ProjectDetailResponse>(key, ct);
        if (cached is not null) return cached;

        var project = await _db.Projects
            .AsNoTracking()
            .Include(p => p.Tasks)
            .FirstOrDefaultAsync(p => p.Id == id && p.OwnerId == userId, ct);

        if (project is null) return null;

        var response = project.ToDetailResponse();
        await _cache.SetAsync(key, response, ProjectsCacheTtl, ct);
        return response;
    }

    public async Task<ProjectResponse> CreateAsync(Guid userId, string name, string? description, CancellationToken ct)
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

        _logger.LogInformation(
            "Created project {ProjectId} with name {ProjectName} for user {UserId}", project.Id, project.Name, userId);

        return project.ToResponse();
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

        await _cache.RemoveAsync(CacheKeys.ProjectsForUser(userId), ct);
        await _cache.RemoveAsync(CacheKeys.ProjectDetail(id, userId), ct);

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

    public async Task ArchiveAsync(Guid id, Guid userId, CancellationToken ct)
    {
        var project = await _db.Projects
            .FirstOrDefaultAsync(p => p.Id == id && p.OwnerId == userId, ct);

        if (project is null)
            throw new NotFoundException("Project", id);

        var openTasks = await _db.Tasks
            .CountAsync(t => t.ProjectId == id && t.Status != TaskItemStatus.Completed
                                                && t.Status != TaskItemStatus.Cancelled, ct);

        if (openTasks > 0)
            throw new ConflictException(
                $"Cannot archive project: {openTasks} open task(s) remain. Complete or cancel them first.");

        // (We'd add an IsArchived flag in a real implementation. For now, just log it.)
        var logger = (ILogger<ProjectService>?)null; // placeholder — we'll inject below
    }

    private static IQueryable<Project> ApplySort(IQueryable<Project> query, string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort))
            return query.OrderByDescending(p => p.CreatedAtUtc);

        var descending = sort.StartsWith('-');
        var field = descending ? sort[1..] : sort;

        return (field.ToLowerInvariant(), descending) switch
        {
            ("name", false) => query.OrderBy(p => p.Name),
            ("name", true) => query.OrderByDescending(p => p.Name),
            ("createdat", false) => query.OrderBy(p => p.CreatedAtUtc),
            ("createdat", true) => query.OrderByDescending(p => p.CreatedAtUtc),
            _ => query.OrderByDescending(p => p.CreatedAtUtc)
        };
    }
}
