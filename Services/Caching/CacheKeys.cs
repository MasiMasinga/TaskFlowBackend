namespace TaskFlow.Services.Caching;

public static class CacheKeys
{
    public static string ProjectsForUser(Guid userId) => $"projects:user:{userId}";
    public static string ProjectDetail(Guid projectId, Guid userId) => $"projects:detail:{projectId}:user:{userId}";
    public static string ProjectsPrefixForUser(Guid userId) => $"projects:user:{userId}";
}