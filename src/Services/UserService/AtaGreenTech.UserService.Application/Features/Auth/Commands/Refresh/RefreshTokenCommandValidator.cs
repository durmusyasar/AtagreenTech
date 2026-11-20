using AtaGreenTech.SharedKernel.Localization;
using FluentValidation;

namespace AtaGreenTech.UserService.Application.Features.Auth.Commands.Refresh
{
    /// <summary>
    /// Refresh isteği alan doğrulaması. Mesajlar <see cref="IErrorLocalizer"/> / <see cref="ErrorCodes"/> ile üretilir (#71).
    /// </summary>
    internal sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
    {
        /// <summary>İstemci gövdesindeki plaintext refresh üst sınırı (DB’de yalnızca hash saklanır).</summary>
        public const int RefreshTokenMaxLength = 512;

        public RefreshTokenCommandValidator(IErrorLocalizer localizer)
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