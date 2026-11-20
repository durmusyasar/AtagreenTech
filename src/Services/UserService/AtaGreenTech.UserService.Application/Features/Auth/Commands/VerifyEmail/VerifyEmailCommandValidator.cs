using AtaGreenTech.SharedKernel.Localization;
using FluentValidation;

namespace AtaGreenTech.UserService.Application.Features.Auth.Commands.VerifyEmail
{
    /// <summary>
    /// E-posta doğrulama isteği alan doğrulaması. Mesajlar <see cref="IErrorLocalizer"/> / <see cref="ErrorCodes"/> (#71).
    /// </summary>
    internal sealed class VerifyEmailCommandValidator : AbstractValidator<VerifyEmailCommand>
    {
        /// <summary>İstemci gövdesindeki plaintext doğrulama jetonu üst sınırı (DB’de yalnızca hash saklanır).</summary>
        public const int TokenMaxLength = 512;

        public VerifyEmailCommandValidator(IErrorLocalizer localizer)
        {
            RuleFor(x => x.Token)
                .NotEmpty()
                .WithRequiredMessage(localizer, ErrorCodes.Fields.Token)
                .MaximumLength(TokenMaxLength)
                .WithMaxLengthMessage(localizer, ErrorCodes.Fields.Token, TokenMaxLength);
        }
    }
}