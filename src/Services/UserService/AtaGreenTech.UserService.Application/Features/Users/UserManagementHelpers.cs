using AtaGreenTech.UserService.Application.DTOs;
using AtaGreenTech.UserService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AtaGreenTech.UserService.Application.Features.Users;

internal static class UserManagementHelpers
{
    private static readonly string[] KnownRoleNames =
    [
        Role.Names.Admin,
        Role.Names.Manager,
        Role.Names.Viewer,
    ];

    public static string ResolveUsername(string email)
    {
        if (email.Length <= User.UsernameMaxLength)
            return email;

        var local = email.Split('@')[0];
        return local.Length <= User.UsernameMaxLength
            ? local
            : local[..User.UsernameMaxLength];
    }

    public static IReadOnlyList<string> NormalizeRoleNames(IEnumerable<string> roles) =>
        roles
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .Select(Role.NormalizeName)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

    public static async Task<IReadOnlyList<Role>> ResolveRolesAsync(
        IQueryable<Role> roles,
        IEnumerable<string> requestedNames,
        CancellationToken cancellationToken
    )
    {
        var names = NormalizeRoleNames(requestedNames);
        if (names.Count == 0)
            return [];

        var known = await roles
            .Where(r => KnownRoleNames.Contains(r.Name))
            .ToListAsync(cancellationToken);

        return names
            .Select(n => known.FirstOrDefault(r => r.Name.Equals(n, StringComparison.OrdinalIgnoreCase)))
            .Where(r => r is not null)
            .Cast<Role>()
            .ToArray();
    }

    public static UserDto ToDto(User user)
    {
        var roleNames = user.UserRoles
            .Where(ur => !ur.IsDeleted && ur.Role is { IsDeleted: false })
            .Select(ur => ur.Role.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(r => r, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return new UserDto(
            user.Id,
            user.Email,
            user.Username,
            user.FirstName,
            user.LastName,
            user.IsActive,
            user.IsEmailVerified,
            user.CreatedAt,
            roleNames
        );
    }

    public static IQueryable<UserDto> ProjectToDto(IQueryable<User> users) =>
        users.Select(u => new UserDto(
            u.Id,
            u.Email,
            u.Username,
            u.FirstName,
            u.LastName,
            u.IsActive,
            u.IsEmailVerified,
            u.CreatedAt,
            u.UserRoles
                .Where(ur => !ur.IsDeleted && ur.Role != null && !ur.Role.IsDeleted)
                .Select(ur => ur.Role.Name)
                .Distinct()
                .OrderBy(n => n)
                .ToList()
        ));
}
