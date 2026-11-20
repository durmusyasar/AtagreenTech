using AtaGreenTech.SharedKernel.Localization;
using AtaGreenTech.UserService.Domain;
using AtaGreenTech.UserService.Domain.Entities;

namespace AtaGreenTech.UserService.Tests.Domain;

public sealed class UserRoleTests
{
    [Fact]
    public void Ctor_accepts_non_empty_ids()
    {
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var link = new UserRole(userId, roleId);

        Assert.Equal(userId, link.UserId);
        Assert.Equal(roleId, link.RoleId);
    }

    [Fact]
    public void Ctor_rejects_empty_user_id()
    {
        var ex = Assert.Throws<ArgumentException>(() => new UserRole(Guid.Empty, Guid.NewGuid()));
        Assert.StartsWith(DomainErrorCodes.ValidationRequired, ex.Message, StringComparison.Ordinal);
        Assert.Equal("userId", ex.ParamName);
        Assert.Equal(ErrorCodes.ValidationRequired, DomainErrorCodes.ValidationRequired);
    }

    [Fact]
    public void Ctor_rejects_empty_role_id()
    {
        var ex = Assert.Throws<ArgumentException>(() => new UserRole(Guid.NewGuid(), Guid.Empty));
        Assert.StartsWith(DomainErrorCodes.ValidationRequired, ex.Message, StringComparison.Ordinal);
        Assert.Equal("roleId", ex.ParamName);
    }
}
