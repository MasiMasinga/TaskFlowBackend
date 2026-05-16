using FluentValidation;
using TaskFlow.DTOs.Auth;

namespace TaskFlow.Api.Validators;

public class LogoutRequestValidator : AbstractValidator<LogoutRequest>
{
    public LogoutRequestValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}
