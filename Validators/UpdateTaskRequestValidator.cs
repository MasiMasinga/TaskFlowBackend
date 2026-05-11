using FluentValidation;
using TaskFlow.DTOs.Tasks;

namespace TaskFlow.Validators;

public class UpdateTaskRequestValidator : AbstractValidator<UpdateTaskRequest>
{
    public UpdateTaskRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Task title is required.")
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .MaximumLength(4000);

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid task status.");

        RuleFor(x => x.DueDateUtc)
            .GreaterThan(DateTime.UtcNow)
            .When(x => x.DueDateUtc.HasValue)
            .WithMessage("Due date must be in the future.");
    }
}