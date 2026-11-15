using AtaGreenTech.UserService.Application.Interfaces;
using AtaGreenTech.UserService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AtaGreenTech.UserService.Application.Features.Auth;

internal static class UserPermissionResolver
{
    public static async Task<IReadOnlyList<string>> ResolveAsync(
        IUserDbContext db,
        Guid userId,
        CancellationToken cancellationToken
    )
    {
        return await db.UserRoles
            .Where(ur => ur.UserId == userId)
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.Name)
            .Distinct()
            .OrderBy(n => n)
            .ToListAsync(cancellationToken);
    }

    public static IReadOnlyList<string> FromLoadedUser(User user) =>
        user.UserRoles
            .Where(ur => !ur.IsDeleted && ur.Role is { IsDeleted: false })
            .SelectMany(ur => ur.Role.RolePermissions)
            .Where(rp => !rp.IsDeleted && rp.Permission is { IsDeleted: false })
            .Select(rp => rp.Permission.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
            .ToArray();
}
