using AtaGreenTech.SharedKernel.Security;
using AtaGreenTech.UserService.Application.Features.Auth;
using AtaGreenTech.UserService.Domain.Entities;
using AtaGreenTech.UserService.Tests.Fakes;

namespace AtaGreenTech.UserService.Tests.Application;

public sealed class RbacCatalogTests
{
    [Fact]
    public async Task EnsureAsync_seeds_catalog_roles_and_grants_on_empty_store()
    {
        var roles = new InMemoryRepository<Role>();
        var permissions = new InMemoryRepository<Permission>();
        var rolePermissions = new InMemoryRepository<RolePermission>();
        var uow = new InMemoryUnitOfWork();

        await RbacCatalog.EnsureAsync(roles, permissions, rolePermissions, uow, CancellationToken.None);

        Assert.Equal(Permissions.Catalog.Count, permissions.Items.Count);
        Assert.Equal(Permissions.DefaultRoleGrants.Count, roles.Items.Count);
        Assert.All(roles.Items, r => Assert.NotEqual(Guid.Empty, r.Id));

        var expectedGrantCount = Permissions.DefaultRoleGrants.Sum(g => g.Value.Length);
        Assert.Equal(expectedGrantCount, rolePermissions.Items.Count);
        Assert.True(uow.SaveChangesCalls >= 2);
    }

    [Fact]
    public async Task EnsureAsync_second_call_does_not_duplicate_rows()
    {
        var roles = new InMemoryRepository<Role>();
        var permissions = new InMemoryRepository<Permission>();
        var rolePermissions = new InMemoryRepository<RolePermission>();
        var uow = new InMemoryUnitOfWork();

        await RbacCatalog.EnsureAsync(roles, permissions, rolePermissions, uow, CancellationToken.None);
        var permissionCount = permissions.Items.Count;
        var roleCount = roles.Items.Count;
        var grantCount = rolePermissions.Items.Count;

        await RbacCatalog.EnsureAsync(roles, permissions, rolePermissions, uow, CancellationToken.None);

        Assert.Equal(permissionCount, permissions.Items.Count);
        Assert.Equal(roleCount, roles.Items.Count);
        Assert.Equal(grantCount, rolePermissions.Items.Count);
    }

    [Fact]
    public async Task EnsureAsync_adds_missing_role_permission_grants()
    {
        var roles = new InMemoryRepository<Role>();
        var permissions = new InMemoryRepository<Permission>();
        var rolePermissions = new InMemoryRepository<RolePermission>();
        var uow = new InMemoryUnitOfWork();

        await RbacCatalog.EnsureAsync(roles, permissions, rolePermissions, uow, CancellationToken.None);

        var viewer = roles.Items.Single(r => r.Name == Role.Names.Viewer);
        var stockRead = permissions.Items.Single(p => p.Name == Permissions.Stock.Read);
        var link = rolePermissions.Items.Single(rp =>
            rp.RoleId == viewer.Id && rp.PermissionId == stockRead.Id
        );
        rolePermissions.Delete(link);

        await RbacCatalog.EnsureAsync(roles, permissions, rolePermissions, uow, CancellationToken.None);

        Assert.Contains(
            rolePermissions.Items,
            rp => rp.RoleId == viewer.Id && rp.PermissionId == stockRead.Id
        );
    }
}
