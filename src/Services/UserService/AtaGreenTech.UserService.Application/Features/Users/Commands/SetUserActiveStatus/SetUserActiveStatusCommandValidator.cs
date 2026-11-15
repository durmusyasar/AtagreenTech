using FluentValidation;

namespace AtaGreenTech.UserService.Application.Features.Users.Commands.SetUserActiveStatus;

internal sealed class SetUserActiveStatusCommandValidator : AbstractValidator<SetUserActiveStatusCommand>
{
    public SetUserActiveStatusCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
