using TaskFlow.Models.Pagination;
using TaskFlow.Enum;

namespace TaskFlow.DTOs.Tasks;

public class TaskListRequest : PagedRequest
{
    public TaskItemStatus? Status { get; set; }
    public DateTime? DueBeforeUtc { get; set; }
    public DateTime? DueAfterUtc { get; set; }
    public string? Search { get; set; }
    public string? Sort { get; set; }  // e.g. "dueDate", "-createdAt", "title"
}