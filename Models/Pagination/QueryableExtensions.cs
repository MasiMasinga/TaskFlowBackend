using Microsoft.EntityFrameworkCore;

namespace TaskFlow.Models.Pagination;

public static class QueryableExtensions
{
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query,
        int page,
        int pageSize,
        CancellationToken ct)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 1;

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<T>(items, page, pageSize, totalCount);
    }
}