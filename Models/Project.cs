namespace TaskFlow.Models;

public class Project : BaseEntity 
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAtUtc { get; set; }

    public Guid OwnerId { get; set; }
    public ApplicationUser Owner { get; set; } = null!;

    public List<TaskItem> Tasks { get; set; } = new();
}