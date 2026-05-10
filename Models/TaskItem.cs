using TaskFlow.Enum;

namespace TaskFlow.Models;

public class TaskItem
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskItemStatus Status { get; set; } = TaskItemStatus.Open;
    public DateTime? DueDateUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
}