using AtaGreenTech.SharedKernel.Localization;
using AtaGreenTech.UserService.Domain.Entities;
using FluentValidation;

namespace AtaGreenTech.UserService.Application.Features.Auth.Commands.Login
{
    /// <summary>
    /// Login isteği alan doğrulaması. Mesajlar <see cref="IErrorLocalizer"/> / <see cref="ErrorCodes"/> ile üretilir (#71).
    /// </summary>
    internal sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator(IErrorLocalizer localizer)
        {
            RuleFor(x => x.Email)
                .NotEmpty()
                .WithRequiredMessage(localizer, ErrorCodes.Fields.Email)
                .EmailAddress()
                .WithLocalizedMessage(
                    localizer,
                    ErrorCodes.ValidationInvalid,
                    localizer.Localize(ErrorCodes.Fields.Email)
                )
                .MaximumLength(User.EmailMaxLength)
                .WithMaxLengthMessage(localizer, ErrorCodes.Fields.Email, User.EmailMaxLength);

            RuleFor(x => x.Password)
                .NotEmpty()
                .WithRequiredMessage(localizer, ErrorCodes.Fields.Password)
                .MaximumLength(PasswordRules.MaxLength)
                .WithMaxLengthMessage(localizer, ErrorCodes.Fields.Password, PasswordRules.MaxLength);
        }
    }
}