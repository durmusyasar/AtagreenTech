using FluentValidation;

namespace AtaGreenTech.UserService.Application.Features.Auth.Commands.ResendVerification;

internal sealed class ResendVerificationCommandValidator : AbstractValidator<ResendVerificationCommand>
{
    public ResendVerificationCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
    }
}
