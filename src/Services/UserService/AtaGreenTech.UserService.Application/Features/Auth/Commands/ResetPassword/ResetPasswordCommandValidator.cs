using AtaGreenTech.UserService.Application.Features.Auth;
using FluentValidation;

namespace AtaGreenTech.UserService.Application.Features.Auth.Commands.ResetPassword;

internal sealed class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Token).NotEmpty().MaximumLength(512);
        RuleFor(x => x.NewPassword).ApplyStrongPassword();
    }
}
