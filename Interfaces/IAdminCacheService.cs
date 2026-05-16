namespace TaskFlow.Interfaces;

public interface IAdminCacheService
{
    Task<int> ClearForUserAsync(Guid userId, CancellationToken ct);
}
