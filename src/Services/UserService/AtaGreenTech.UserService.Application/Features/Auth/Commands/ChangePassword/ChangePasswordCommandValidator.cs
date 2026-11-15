using AtaGreenTech.UserService.Application.Features.Auth;
using FluentValidation;

namespace AtaGreenTech.UserService.Application.Features.Auth.Commands.ChangePassword;

internal sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.CurrentPassword).NotEmpty().MaximumLength(PasswordRules.MaxLength);
        RuleFor(x => x.NewPassword).ApplyStrongPassword();
        RuleFor(x => x.NewPassword)
            .NotEqual(x => x.CurrentPassword)
            .WithMessage("Yeni şifre mevcut şifre ile aynı olamaz.");
    }
}
