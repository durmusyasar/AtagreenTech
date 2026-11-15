using FluentValidation;

namespace AtaGreenTech.UserService.Application.Features.Auth.Commands.VerifyEmail;

internal sealed class VerifyEmailCommandValidator : AbstractValidator<VerifyEmailCommand>
{
    public VerifyEmailCommandValidator()
    {
        RuleFor(x => x.Token).NotEmpty().MaximumLength(512);
    }
}
