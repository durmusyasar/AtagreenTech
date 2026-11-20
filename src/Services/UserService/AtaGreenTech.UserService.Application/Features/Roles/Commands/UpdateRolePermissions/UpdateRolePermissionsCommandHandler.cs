using AtaGreenTech.UserService.Application.DTOs;
using AtaGreenTech.UserService.Application.Features.Auth;
using AtaGreenTech.UserService.Application.Interfaces;
using AtaGreenTech.UserService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using OmniSync.Core.Application.Messaging;
using OmniSync.Core.Domain.Exceptions;
using OmniSync.Core.Domain.Repositories;
using OmniSync.Core.Domain.Tenants;
using OmniSync.Core.Domain.Users;

namespace AtaGreenTech.UserService.Application.Features.Roles.Commands.UpdateRolePermissions;

internal sealed class UpdateRolePermissionsCommandHandler(
    IRepository<Role> roles,
    IRepository<RolePermission> rolePermissions,
    IUserDbContext db,
    IIdentityAuditLogger auditLogger,
    ICurrentUser currentUser,
    ICurrentTenant currentTenant,
    INavigationMenuCache navigationMenuCache,
    AccessTokenLifecycle accessTokenLifecycle
) : ICommandHandler<UpdateRolePermissionsCommand, RoleWriteResult>
{
    public async Task<RoleWriteResult> Handle(
        UpdateRolePermissionsCommand request,
        CancellationToken cancellationToken
    )
    {
        var role = await roles.GetByIdAsync(request.RoleId, cancellationToken)
            ?? throw new NotFoundException(nameof(Role), request.RoleId);

        if (string.Equals(role.Name, Role.Names.Admin, StringComparison.OrdinalIgnoreCase))
            return RoleWriteResult.Fail("Admin rolünün izinleri değiştirilemez.");

        var desiredNames = request.Permissions
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Select(Permission.NormalizeName)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var known = await db.Permissions
            .Where(p => desiredNames.Contains(p.Name))
            .ToListAsync(cancellationToken);

        if (known.Count != desiredNames.Length)
        {
            var knownNames = known.Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var missing = desiredNames.Where(n => !knownNames.Contains(n));
            return RoleWriteResult.Fail($"Bilinmeyen izin(ler): {string.Join(", ", missing)}.");
        }

        var existing = await rolePermissions.GetAllByFilterAsync(
            rp => rp.RoleId == role.Id,
            cancellationToken
        );
        var desiredIds = known.Select(p => p.Id).ToHashSet();
        var existingIds = existing.Select(rp => rp.PermissionId).ToHashSet();

        foreach (var assignment in existing.Where(rp => !desiredIds.Contains(rp.PermissionId)))
            rolePermissions.Delete(assignment);

        foreach (var permission in known.Where(p => !existingIds.Contains(p.Id)))
            await rolePermissions.AddAsync(new RolePermission(role.Id, permission.Id), cancellationToken);

        var permissionNames = known
            .Select(p => p.Name)
            .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var affectedUsers = await db.Users
            .Where(u => u.UserRoles.Any(ur => ur.RoleId == role.Id && !ur.IsDeleted))
            .ToListAsync(cancellationToken);

        foreach (var user in affectedUsers)
        {
            user.InvalidateCredentialStamp();
            await accessTokenLifecycle.PublishStampAsync(user.Id, user.SecurityStamp, cancellationToken);
        }

        Guid? actorId = Guid.TryParse(currentUser.UserId, out var id) ? id : null;
        var actorEmail = string.IsNullOrWhiteSpace(currentUser.Email) ? "unknown" : currentUser.Email;
        await auditLogger.LogAsync(
            AuditLog.Actions.RolePermissionsChanged,
            actorEmail,
            actorId,
            request.ClientIp,
            $"{role.Name} rolünün izinleri güncellendi: {string.Join(", ", permissionNames)}.",
            cancellationToken
        );

        if (currentTenant.Id is { } tenantId)
            navigationMenuCache.InvalidateTenant(tenantId);

        return RoleWriteResult.Ok(
            new RoleDetailDto(role.Id, role.Name, role.Description, permissionNames),
            "Rol izinleri güncellendi."
        );
    }
}
