using TaskFlow.Enum;

namespace TaskFlow.Models;

public class TaskItem : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskItemStatus Status { get; set; } = TaskItemStatus.Open;
    public Priority Priority { get; set; } = Priority.Medium;
    public DateTime? DueDateUtc { get; set; }
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
}