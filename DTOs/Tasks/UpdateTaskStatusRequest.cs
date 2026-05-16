using TaskFlow.Enum;

namespace TaskFlow.DTOs.Tasks;

public sealed record UpdateTaskStatusRequest(TaskItemStatus Status);
