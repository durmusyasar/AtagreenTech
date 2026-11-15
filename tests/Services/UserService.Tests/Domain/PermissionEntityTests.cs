using AtaGreenTech.SharedKernel.Security;
using AtaGreenTech.UserService.Domain.Entities;

namespace AtaGreenTech.UserService.Tests.Domain;

public sealed class PermissionEntityTests
{
    [Fact]
    public void Permission_normalizes_name_to_lowercase()
    {
        var permission = new Permission("Users.Read", "desc");

        Assert.Equal(Permissions.Users.Read, permission.Name);
        Assert.Equal("desc", permission.Description);
    }

    [Fact]
    public void RolePermission_requires_ids()
    {
        Assert.Throws<ArgumentException>(() => new RolePermission(Guid.Empty, Guid.NewGuid()));
        Assert.Throws<ArgumentException>(() => new RolePermission(Guid.NewGuid(), Guid.Empty));

        var roleId = Guid.NewGuid();
        var permissionId = Guid.NewGuid();
        var link = new RolePermission(roleId, permissionId);

        Assert.Equal(roleId, link.RoleId);
        Assert.Equal(permissionId, link.PermissionId);
    }

    [Fact]
    public void Catalog_covers_default_role_grants()
    {
        var catalog = Permissions.Catalog.Select(c => c.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var grant in Permissions.DefaultRoleGrants.SelectMany(g => g.Value))
            Assert.Contains(grant, catalog);
    }
}
