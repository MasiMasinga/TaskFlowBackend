using FluentValidation;
using TaskFlow.DTOs.Tasks;
using TaskFlow.Enum;

namespace TaskFlow.Validators;

public sealed class UpdateTaskStatusRequestValidator : AbstractValidator<UpdateTaskStatusRequest>
{
    public UpdateTaskStatusRequestValidator(IValidator<TaskItemStatus> statusValidator)
    {
        RuleFor(x => x.Status).SetValidator(statusValidator);
    }
}
