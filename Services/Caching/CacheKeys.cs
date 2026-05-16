namespace TaskFlow.Services.Caching;

public static class CacheKeys
{
    public const string InstanceName = "taskflow:";

    public static string RedisPatternForUser(Guid userId) => $"{InstanceName}*:user:{userId}*";
    public static string ProjectsForUser(Guid userId) => $"projects:user:{userId}";
    public static string ProjectDetail(Guid projectId, Guid userId) => $"projects:detail:{projectId}:user:{userId}";
    public static string ProjectsPrefixForUser(Guid userId) => $"projects:user:{userId}";
    public static string TasksForProject(Guid projectId, Guid userId) => $"tasks:project:{projectId}:user:{userId}";
    public static string TaskDetail(Guid taskId, Guid userId) => $"tasks:detail:{taskId}:user:{userId}";
    public static string TasksPrefixForProject(Guid projectId, Guid userId) => $"tasks:project:{projectId}:user:{userId}";
}