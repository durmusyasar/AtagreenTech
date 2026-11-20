using AtaGreenTech.SharedKernel.Localization;
using FluentValidation;

namespace AtaGreenTech.UserService.Application.Features.Auth.Commands.ResetPassword
{
    /// <summary>
    /// Şifre sıfırlama isteği alan doğrulaması. Mesajlar <see cref="IErrorLocalizer"/> / <see cref="ErrorCodes"/> (#71).
    /// </summary>
    internal sealed class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
    {
        /// <summary>İstemci gövdesindeki plaintext reset jetonu üst sınırı (DB’de yalnızca hash saklanır).</summary>
        public const int TokenMaxLength = 512;

        public ResetPasswordCommandValidator(IErrorLocalizer localizer)
        {
            RuleFor(x => x.Token)
                .NotEmpty()
                .WithRequiredMessage(localizer, ErrorCodes.Fields.Token)
                .MaximumLength(TokenMaxLength)
                .WithMaxLengthMessage(localizer, ErrorCodes.Fields.Token, TokenMaxLength);

            RuleFor(x => x.NewPassword).ApplyStrongPassword(localizer);
        }
    }
}