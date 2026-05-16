using FluentValidation;
using TaskFlow.Enum;

namespace TaskFlow.Validators;

public sealed class TaskItemStatusValidator : AbstractValidator<TaskItemStatus>
{
    public TaskItemStatusValidator()
    {
        RuleFor(x => x)
            .IsInEnum()
            .WithMessage("Invalid task status.");
    }
}
