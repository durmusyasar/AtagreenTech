using AtaGreenTech.SharedKernel.Localization;
using FluentValidation;

namespace AtaGreenTech.UserService.Application.Features.Auth.Commands.DisableMfa
{
    /// <summary>
    /// MFA kapatma isteği alan doğrulaması. Mesajlar <see cref="IErrorLocalizer"/> / <see cref="ErrorCodes"/> (#71).
    /// </summary>
    internal sealed class DisableMfaCommandValidator : AbstractValidator<DisableMfaCommand>
    {
        public DisableMfaCommandValidator(IErrorLocalizer localizer)
        {
            RuleFor(x => x.Password)
                .NotEmpty()
                .WithRequiredMessage(localizer, ErrorCodes.Fields.Password)
                .MaximumLength(PasswordRules.MaxLength)
                .WithMaxLengthMessage(localizer, ErrorCodes.Fields.Password, PasswordRules.MaxLength);
        }
    }
}