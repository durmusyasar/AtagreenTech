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

    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(User.NameMaxLength);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(User.NameMaxLength);
        RuleFor(x => x.Roles)
            .Must(roles => roles is null || roles.Count == 0 || roles.All(r =>
                AllowedRoles.Contains(Role.NormalizeName(r), StringComparer.OrdinalIgnoreCase)))
            .WithMessage($"Roles must be one or more of: {string.Join(", ", AllowedRoles)}.");
        RuleFor(x => x.Roles)
            .Must(roles => roles is null || roles.Count > 0)
            .WithMessage("En az bir geçerli rol seçilmelidir.")
            .When(x => x.Roles is not null);
    }
}
