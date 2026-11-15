using FluentValidation;

namespace AtaGreenTech.UserService.Application.Features.Sessions.Commands.RevokeOtherSessions;

internal sealed class RevokeOtherSessionsCommandValidator : AbstractValidator<RevokeOtherSessionsCommand>
{
    public RevokeOtherSessionsCommandValidator()
    {
        RuleFor(x => x.CurrentRefreshToken).NotEmpty().MaximumLength(512);
    }
}
