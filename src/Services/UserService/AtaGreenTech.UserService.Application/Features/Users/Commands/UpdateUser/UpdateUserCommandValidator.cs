using AtaGreenTech.SharedKernel.Localization;
using AtaGreenTech.UserService.Domain.Entities;
using FluentValidation;

namespace AtaGreenTech.UserService.Application.Features.Users.Commands.UpdateUser;

internal sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    private static readonly string[] AllowedRoles =
    [
        Role.Names.Admin,
        Role.Names.Manager,
        Role.Names.Viewer,
    ];

    public UpdateUserCommandValidator(IErrorLocalizer localizer)
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(User.NameMaxLength);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(User.NameMaxLength);
        RuleFor(x => x.Roles)
            .Must(roles => roles is null || roles.Count == 0 || roles.All(r =>
                AllowedRoles.Contains(Role.NormalizeName(r), StringComparer.OrdinalIgnoreCase)))
            .WithLocalizedMessage(
                localizer,
                ErrorCodes.UserRolesInvalid,
                string.Join(", ", AllowedRoles)
            );
        RuleFor(x => x.Roles)
            .Must(roles => roles is null || roles.Count > 0)
            .WithLocalizedMessage(localizer, ErrorCodes.UserRoleRequired)
            .When(x => x.Roles is not null);
    }
}
