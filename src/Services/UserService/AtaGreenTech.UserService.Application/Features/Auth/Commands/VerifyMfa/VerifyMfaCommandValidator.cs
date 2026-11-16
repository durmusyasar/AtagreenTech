using FluentValidation;

namespace AtaGreenTech.UserService.Application.Features.Auth.Commands.VerifyMfa;

internal sealed class VerifyMfaCommandValidator : AbstractValidator<VerifyMfaCommand>
{
    public VerifyMfaCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(16);
    }
}
