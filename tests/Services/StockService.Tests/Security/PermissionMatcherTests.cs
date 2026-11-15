using AtaGreenTech.SharedKernel.Security;

namespace AtaGreenTech.StockService.Tests.Security;

public sealed class PermissionMatcherTests
{
    [Theory]
    [InlineData(Permissions.Stock.Read, true)]
    [InlineData(Permissions.Stock.Write, false)]
    [InlineData(Permissions.Stock.Delete, false)]
    public void Exact_stock_read_grant(string required, bool expected) =>
        Assert.Equal(expected, PermissionMatcher.Satisfies([Permissions.Stock.Read], required));

    [Fact]
    public void Stock_wildcard_covers_stock_permissions_only()
    {
        var granted = new[] { Permissions.Stock.All };

        Assert.True(PermissionMatcher.Satisfies(granted, Permissions.Stock.Read));
        Assert.True(PermissionMatcher.Satisfies(granted, Permissions.Stock.Write));
        Assert.True(PermissionMatcher.Satisfies(granted, Permissions.Stock.Delete));
        Assert.False(PermissionMatcher.Satisfies(granted, Permissions.Users.Read));
    }

    [Fact]
    public void System_wildcard_covers_all()
    {
        var granted = new[] { Permissions.SystemAll };

        Assert.True(PermissionMatcher.Satisfies(granted, Permissions.Stock.Delete));
        Assert.True(PermissionMatcher.Satisfies(granted, Permissions.Users.Write));
    }

    [Fact]
    public void Empty_grants_deny() =>
        Assert.False(PermissionMatcher.Satisfies([], Permissions.Stock.Read));
}
