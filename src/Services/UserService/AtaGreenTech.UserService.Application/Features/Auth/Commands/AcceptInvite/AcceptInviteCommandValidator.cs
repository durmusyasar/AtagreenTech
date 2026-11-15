using AtaGreenTech.UserService.Application.Features.Auth;
using FluentValidation;

namespace AtaGreenTech.UserService.Application.Features.Auth.Commands.AcceptInvite;

internal sealed class AcceptInviteCommandValidator : AbstractValidator<AcceptInviteCommand>
{
    public AcceptInviteCommandValidator()
    {
        RuleFor(x => x.Token).NotEmpty().MaximumLength(512);
        RuleFor(x => x.Password).ApplyStrongPassword();
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
    }
}
