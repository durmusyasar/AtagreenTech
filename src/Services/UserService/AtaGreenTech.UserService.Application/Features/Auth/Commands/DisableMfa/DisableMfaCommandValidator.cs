using FluentValidation;

namespace AtaGreenTech.UserService.Application.Features.Auth.Commands.DisableMfa;

internal sealed class DisableMfaCommandValidator : AbstractValidator<DisableMfaCommand>
{
    public DisableMfaCommandValidator()
    {
        RuleFor(x => x.Password).NotEmpty().MaximumLength(128);
    }
}
