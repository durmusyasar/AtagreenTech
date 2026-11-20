using AtaGreenTech.SharedKernel.Localization;
using FluentValidation;

namespace AtaGreenTech.UserService.Application.Features.Auth.Commands.ChangePassword
{
    /// <summary>
    /// Şifre değiştirme alan doğrulaması. Mesajlar <see cref="IErrorLocalizer"/> / <see cref="ErrorCodes"/> (#71).
    /// </summary>
    internal sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
    {
        public ChangePasswordCommandValidator(IErrorLocalizer localizer)
        {
            RuleFor(x => x.CurrentPassword)
                .NotEmpty()
                .WithRequiredMessage(localizer, ErrorCodes.Fields.CurrentPassword)
                .MaximumLength(PasswordRules.MaxLength)
                .WithMaxLengthMessage(
                    localizer,
                    ErrorCodes.Fields.CurrentPassword,
                    PasswordRules.MaxLength
                );

            RuleFor(x => x.NewPassword).ApplyStrongPassword(localizer);

            RuleFor(x => x.NewPassword)
                .NotEqual(x => x.CurrentPassword)
                .WithLocalizedMessage(localizer, ErrorCodes.AuthPasswordSameAsCurrent);
        }
    }
}