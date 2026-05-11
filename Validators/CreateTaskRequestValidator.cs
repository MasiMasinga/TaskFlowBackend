using FluentValidation;
using TaskFlow.DTOs.Tasks;

namespace TaskFlow.Validators;

public class CreateTaskRequestValidator : AbstractValidator<CreateTaskRequest>
{
    public CreateTaskRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Task title is required.")
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .MaximumLength(4000);

        RuleFor(x => x.DueDateUtc)
            .GreaterThan(DateTime.UtcNow)
            .When(x => x.DueDateUtc.HasValue)
            .WithMessage("Due date must be in the future.");
    }
}