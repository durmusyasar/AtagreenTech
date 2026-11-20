using System.Reflection;
using AtaGreenTech.UserService.Application.Features.Auth;
using AtaGreenTech.UserService.Domain.Entities;

namespace AtaGreenTech.UserService.Tests.Application;

public sealed class UserPermissionResolverTests
{
    [Fact]
    public void FromLoadedUser_unions_permissions_from_all_active_roles()
    {
        var userId = Guid.NewGuid();
        var user = new User("a@b.com", "admin", "hash", "A", "B");

        var viewer = new Role(Role.Names.Viewer);
        var manager = new Role(Role.Names.Manager);
        AttachPermission(viewer, new Permission("users.read"));
        AttachPermission(viewer, new Permission("stock.read"));
        AttachPermission(manager, new Permission("stock.read"));
        AttachPermission(manager, new Permission("stock.write"));

        AttachUserRole(user, userId, viewer);
        AttachUserRole(user, userId, manager);

        var permissions = UserPermissionResolver.FromLoadedUser(user);

        Assert.Equal(["stock.read", "stock.write", "users.read"], permissions);
    }

    [Fact]
    public void FromLoadedUser_excludes_soft_deleted_assignments_roles_and_permissions()
    {
        var userId = Guid.NewGuid();
        var user = new User("a@b.com", "admin", "hash", "A", "B");

        var activeRole = new Role(Role.Names.Viewer);
        var deletedRole = new Role(Role.Names.Manager);
        AttachPermission(activeRole, new Permission("users.read"));
        var deletedPerm = new Permission("stock.write");
        AttachPermission(activeRole, deletedPerm);
        deletedPerm.SoftDelete();

        AttachPermission(deletedRole, new Permission("stock.delete"));
        deletedRole.SoftDelete();

        var activeLink = AttachUserRole(user, userId, activeRole);
        var deletedLink = AttachUserRole(user, userId, deletedRole);
        deletedLink.SoftDelete();

        var permissions = UserPermissionResolver.FromLoadedUser(user);

        Assert.Equal(["users.read"], permissions);
    }

    private static UserRole AttachUserRole(User user, Guid userId, Role role)
    {
        // User.Id EF tarafından atanır; test grafiğinde UserRole.UserId ile hizala.
        var link = new UserRole(userId, role.Id == Guid.Empty ? Guid.NewGuid() : role.Id);
        SetNav(link, nameof(UserRole.Role), role);
        SetNav(link, nameof(UserRole.User), user);
        if (role.Id == Guid.Empty)
            SetId(role, link.RoleId);
        AddToPrivateList(user, "_userRoles", link);
        return link;
    }

    private static void AttachPermission(Role role, Permission permission)
    {
        if (permission.Id == Guid.Empty)
            SetId(permission, Guid.NewGuid());
        if (role.Id == Guid.Empty)
            SetId(role, Guid.NewGuid());

        var link = new RolePermission(role.Id, permission.Id);
        SetNav(link, nameof(RolePermission.Permission), permission);
        SetNav(link, nameof(RolePermission.Role), role);
        AddToPrivateList(role, "_rolePermissions", link);
    }

    private static void SetNav(object target, string propertyName, object value) =>
        target.GetType().GetProperty(propertyName)!.SetValue(target, value);

    private static void SetId(object entity, Guid id) =>
        entity.GetType().GetProperty("Id")!.SetValue(entity, id);

    private static void AddToPrivateList(object owner, string fieldName, object item)
    {
        var field = owner.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException($"Field {fieldName} not found.");
        var list = field.GetValue(owner) ?? throw new InvalidOperationException("List null.");
        list.GetType().GetMethod("Add")!.Invoke(list, [item]);
    }
}
