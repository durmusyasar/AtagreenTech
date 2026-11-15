using FluentValidation;

namespace AtaGreenTech.UserService.Application.Features.Auth.Commands.ChangePassword;

internal sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.CurrentPassword).NotEmpty().MaximumLength(128);
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(8).MaximumLength(128)
            .NotEqual(x => x.CurrentPassword)
            .WithMessage("Yeni şifre mevcut şifre ile aynı olamaz.");
    }
}
