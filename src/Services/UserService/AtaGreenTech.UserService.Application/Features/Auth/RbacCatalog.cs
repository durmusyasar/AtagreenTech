using AtaGreenTech.SharedKernel.Security;
using AtaGreenTech.UserService.Domain.Entities;
using OmniSync.Core.Domain.Repositories;

namespace AtaGreenTech.UserService.Application.Features.Auth;

/// <summary>İzin kataloğu ve varsayılan rol eşlemelerini idempotent olarak sağlar.</summary>
internal static class RbacCatalog
{
    public static async Task EnsureAsync(
        IRepository<Role> roles,
        IRepository<Permission> permissions,
        IRepository<RolePermission> rolePermissions,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken
    )
    {
        foreach (var (name, description) in Permissions.Catalog)
            await EnsurePermissionAsync(permissions, name, description, cancellationToken);

        var roleByName = new Dictionary<string, Role>(StringComparer.OrdinalIgnoreCase);
        foreach (var (roleName, _) in Permissions.DefaultRoleGrants)
        {
            var description = roleName switch
            {
                Role.Names.Admin => "System Administrator",
                Role.Names.Manager => "Tenant manager",
                Role.Names.Viewer => "Read-only viewer",
                _ => $"{roleName} Role",
            };
            roleByName[roleName] = await EnsureRoleAsync(roles, roleName, description, cancellationToken);
        }

        // Roller/izinler AsNoTracking login sorgusunda görünsün diye ara kayıt.
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var allPermissions = await permissions.GetAllByFilterAsync(_ => true, cancellationToken);
        var permissionByName = allPermissions.ToDictionary(
            p => p.Name,
            StringComparer.OrdinalIgnoreCase
        );

        foreach (var (roleName, grantNames) in Permissions.DefaultRoleGrants)
        {
            var role = roleByName[roleName];
            var existing = await rolePermissions.GetAllByFilterAsync(
                rp => rp.RoleId == role.Id,
                cancellationToken
            );
            var existingIds = existing.Select(rp => rp.PermissionId).ToHashSet();

            foreach (var grantName in grantNames)
            {
                var key = Permission.NormalizeName(grantName);
                if (!permissionByName.TryGetValue(key, out var permission))
                    continue;

                if (existingIds.Contains(permission.Id))
                    continue;

                await rolePermissions.AddAsync(new RolePermission(role.Id, permission.Id), cancellationToken);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static async Task EnsurePermissionAsync(
        IRepository<Permission> permissions,
        string name,
        string description,
        CancellationToken cancellationToken
    )
    {
        var normalized = Permission.NormalizeName(name);
        var existing = (await permissions.GetAllByFilterAsync(p => p.Name == normalized, cancellationToken))
            .FirstOrDefault();
        if (existing is not null)
            return;

        await permissions.AddAsync(new Permission(normalized, description), cancellationToken);
    }

    private static async Task<Role> EnsureRoleAsync(
        IRepository<Role> roles,
        string name,
        string description,
        CancellationToken cancellationToken
    )
    {
        var existing = (await roles.GetAllByFilterAsync(r => r.Name == name, cancellationToken)).FirstOrDefault();
        if (existing is not null)
            return existing;

        return await roles.AddAsync(new Role(name, description), cancellationToken);
    }
}
