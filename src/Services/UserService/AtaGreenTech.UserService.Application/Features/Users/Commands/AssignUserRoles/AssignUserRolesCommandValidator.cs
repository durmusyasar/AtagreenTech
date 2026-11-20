using AtaGreenTech.SharedKernel.Localization;
using AtaGreenTech.UserService.Domain.Entities;
using FluentValidation;

namespace AtaGreenTech.UserService.Application.Features.Users.Commands.AssignUserRoles;

internal sealed class AssignUserRolesCommandValidator : AbstractValidator<AssignUserRolesCommand>
{
    private static readonly string[] AllowedRoles =
    [
        Role.Names.Admin,
        Role.Names.Manager,
        Role.Names.Viewer,
    ];

    public AssignUserRolesCommandValidator(IErrorLocalizer localizer)
    {
        RuleFor(x => x.UserId).NotEmpty();
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
