using AtaGreenTech.SharedKernel.Localization;
using FluentValidation;

namespace AtaGreenTech.UserService.Application.Features.Auth.Commands.VerifyMfa
{
    /// <summary>
    /// MFA doğrulama isteği alan doğrulaması. Mesajlar <see cref="IErrorLocalizer"/> / <see cref="ErrorCodes"/> (#71).
    /// </summary>
    internal sealed class VerifyMfaCommandValidator : AbstractValidator<VerifyMfaCommand>
    {
        /// <summary>Authenticator / kurtarma kodu üst sınırı (normalize sonrası 6 hane; boşluk toleransı).</summary>
        public const int CodeMaxLength = 16;

        public VerifyMfaCommandValidator(IErrorLocalizer localizer)
        {
            RuleFor(x => x.Code)
                .NotEmpty()
                .WithRequiredMessage(localizer, ErrorCodes.Fields.Code)
                .MaximumLength(CodeMaxLength)
                .WithMaxLengthMessage(localizer, ErrorCodes.Fields.Code, CodeMaxLength);
        }
    }
}