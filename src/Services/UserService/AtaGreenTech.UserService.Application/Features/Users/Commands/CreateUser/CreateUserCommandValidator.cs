using AtaGreenTech.SharedKernel.Localization;
using AtaGreenTech.UserService.Application.Features.Auth;
using AtaGreenTech.UserService.Domain.Entities;
using FluentValidation;

namespace AtaGreenTech.UserService.Application.Features.Users.Commands.CreateUser;

internal sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    private static readonly string[] AllowedRoles =
    [
        Role.Names.Admin,
        Role.Names.Manager,
        Role.Names.Viewer,
    ];

    public CreateUserCommandValidator(IErrorLocalizer localizer)
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(User.EmailMaxLength);
        RuleFor(x => x.Password).ApplyStrongPassword(localizer);
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(User.NameMaxLength);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(User.NameMaxLength);
        RuleFor(x => x.Roles)
            .NotEmpty()
            .Must(roles => roles.All(r =>
                AllowedRoles.Contains(Role.NormalizeName(r), StringComparer.OrdinalIgnoreCase)))
            .WithLocalizedMessage(
                localizer,
                ErrorCodes.UserRolesInvalid,
                string.Join(", ", AllowedRoles)
            );
    }
}
