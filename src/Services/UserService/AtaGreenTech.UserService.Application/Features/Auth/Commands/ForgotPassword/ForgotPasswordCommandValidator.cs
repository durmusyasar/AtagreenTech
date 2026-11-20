using AtaGreenTech.SharedKernel.Localization;
using AtaGreenTech.UserService.Domain.Entities;
using FluentValidation;

namespace AtaGreenTech.UserService.Application.Features.Auth.Commands.ForgotPassword
{
    /// <summary>
    /// Şifremi unuttum isteği alan doğrulaması. Mesajlar <see cref="IErrorLocalizer"/> / <see cref="ErrorCodes"/> (#71).
    /// </summary>
    internal sealed class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
    {
        public ForgotPasswordCommandValidator(IErrorLocalizer localizer)
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
        }
    }
}