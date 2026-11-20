using AtaGreenTech.SharedKernel.Localization;
using AtaGreenTech.UserService.Application.Features.Auth;
using FluentValidation;

namespace AtaGreenTech.UserService.Application.Features.Auth.Commands.ChangePassword;

internal sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator(IErrorLocalizer localizer)
    {
        RuleFor(x => x.CurrentPassword).NotEmpty().MaximumLength(PasswordRules.MaxLength);
        RuleFor(x => x.NewPassword).ApplyStrongPassword(localizer);
        RuleFor(x => x.NewPassword)
            .NotEqual(x => x.CurrentPassword)
            .WithLocalizedMessage(localizer, ErrorCodes.AuthPasswordSameAsCurrent);
    }
}
