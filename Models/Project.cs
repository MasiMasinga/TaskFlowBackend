namespace TaskFlow.Models;

public class Project : BaseEntity 
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<TaskItem> Tasks { get; set; } = new();
}