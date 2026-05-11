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
            .Must(due => !due.HasValue || due.Value >= DateTime.UtcNow.AddYears(-50))
            .WithMessage("Due date, if provided, must not be more than 50 years in the past.");
    }
}