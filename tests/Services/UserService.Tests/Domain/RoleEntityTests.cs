using AtaGreenTech.UserService.Domain;
using AtaGreenTech.UserService.Domain.Entities;

namespace AtaGreenTech.UserService.Tests.Domain;

public sealed class RoleEntityTests
{
    [Fact]
    public void Ctor_normalizes_name_and_description()
    {
        var role = new Role("  Admin  ", "  Tenant admin  ");

        Assert.Equal("Admin", role.Name);
        Assert.Equal("Tenant admin", role.Description);
    }

    [Fact]
    public void Ctor_rejects_blank_name()
    {
        var ex = Assert.Throws<ArgumentException>(() => new Role("  "));
        Assert.StartsWith(DomainErrorCodes.ValidationRequired, ex.Message, StringComparison.Ordinal);
        Assert.Equal("name", ex.ParamName);
    }

    [Fact]
    public void Ctor_rejects_name_over_max_length()
    {
        var tooLong = new string('A', Role.NameMaxLength + 1);
        var ex = Assert.Throws<ArgumentException>(() => new Role(tooLong));
        Assert.StartsWith(DomainErrorCodes.ValidationMaxLength, ex.Message, StringComparison.Ordinal);
        Assert.Equal("name", ex.ParamName);
    }
}
