using OmniSync.Core.Domain.Common;

namespace AtaGreenTech.UserService.Domain.Entities;

/// <summary>
/// Rol–izin ataması. Soft-delete ile geri alınabilir; silinmemiş kayıtlar arasında (RoleId, PermissionId) benzersizdir.
/// </summary>
public class RolePermission : BaseEntity
{
    protected RolePermission()
    {
        Role = null!;
        Permission = null!;
    }

    public RolePermission(Guid roleId, Guid permissionId)
        : this()
    {
        if (roleId == Guid.Empty)
            throw new ArgumentException("Role id is required.", nameof(roleId));
        if (permissionId == Guid.Empty)
            throw new ArgumentException("Permission id is required.", nameof(permissionId));

        RoleId = roleId;
        PermissionId = permissionId;
    }

    public Guid RoleId { get; private set; }

    public Role Role { get; private set; }

    public Guid PermissionId { get; private set; }

    public Permission Permission { get; private set; }
}
