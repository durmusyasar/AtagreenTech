using FluentValidation;

namespace AtaGreenTech.UserService.Application.Features.Sessions.Commands.RevokeSession;

internal sealed class RevokeSessionCommandValidator : AbstractValidator<RevokeSessionCommand>
{
    public RevokeSessionCommandValidator()
    {
        RuleFor(x => x.SessionId).NotEmpty();
    }
}
