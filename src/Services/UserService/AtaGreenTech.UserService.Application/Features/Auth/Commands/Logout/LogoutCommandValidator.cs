using AtaGreenTech.SharedKernel.Localization;
using FluentValidation;

namespace AtaGreenTech.UserService.Application.Features.Auth.Commands.Logout
{
    /// <summary>
    /// Logout isteği alan doğrulaması. Mesajlar <see cref="IErrorLocalizer"/> / <see cref="ErrorCodes"/> ile üretilir (#71).
    /// </summary>
    internal sealed class LogoutCommandValidator : AbstractValidator<LogoutCommand>
    {
        /// <summary>İstemci gövdesindeki plaintext refresh üst sınırı (DB’de yalnızca hash saklanır).</summary>
        public const int RefreshTokenMaxLength = 512;

        public LogoutCommandValidator(IErrorLocalizer localizer)
        {
            RuleFor(x => x.RefreshToken)
                .NotEmpty()
                .WithRequiredMessage(localizer, ErrorCodes.Fields.RefreshToken)
                .MaximumLength(RefreshTokenMaxLength)
                .WithMaxLengthMessage(
                    localizer,
                    ErrorCodes.Fields.RefreshToken,
                    RefreshTokenMaxLength
                );
        }
    }
}