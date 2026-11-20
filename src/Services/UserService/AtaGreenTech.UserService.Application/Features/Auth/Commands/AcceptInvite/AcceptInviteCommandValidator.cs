using AtaGreenTech.SharedKernel.Localization;
using AtaGreenTech.UserService.Domain.Entities;
using FluentValidation;

namespace AtaGreenTech.UserService.Application.Features.Auth.Commands.AcceptInvite
{
    /// <summary>
    /// Davet kabul isteği alan doğrulaması. Mesajlar <see cref="IErrorLocalizer"/> / <see cref="ErrorCodes"/> (#71).
    /// </summary>
    internal sealed class AcceptInviteCommandValidator : AbstractValidator<AcceptInviteCommand>
    {
        /// <summary>İstemci gövdesindeki plaintext davet jetonu üst sınırı (DB’de yalnızca hash saklanır).</summary>
        public const int TokenMaxLength = 512;

        public AcceptInviteCommandValidator(IErrorLocalizer localizer)
        {
            RuleFor(x => x.Token)
                .NotEmpty()
                .WithRequiredMessage(localizer, ErrorCodes.Fields.Token)
                .MaximumLength(TokenMaxLength)
                .WithMaxLengthMessage(localizer, ErrorCodes.Fields.Token, TokenMaxLength);

            RuleFor(x => x.Password).ApplyStrongPassword(localizer);

            RuleFor(x => x.FirstName)
                .NotEmpty()
                .WithRequiredMessage(localizer, ErrorCodes.Fields.FirstName)
                .MaximumLength(User.NameMaxLength)
                .WithMaxLengthMessage(localizer, ErrorCodes.Fields.FirstName, User.NameMaxLength);

            RuleFor(x => x.LastName)
                .NotEmpty()
                .WithRequiredMessage(localizer, ErrorCodes.Fields.LastName)
                .MaximumLength(User.NameMaxLength)
                .WithMaxLengthMessage(localizer, ErrorCodes.Fields.LastName, User.NameMaxLength);
        }
    }
}