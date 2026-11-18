using AtaGreenTech.SharedKernel.Security;
using AtaGreenTech.UserService.Application.Features.Navigation;
using AtaGreenTech.UserService.Application.Interfaces;

namespace AtaGreenTech.UserService.Tests.Application;

public sealed class NavigationMenuTreeBuilderTests
{
    [Fact]
    public void Build_filters_items_without_required_permission()
    {
        var rows = new[]
        {
            Row("iam.security", "UserService", "services.iam", "/account/security", null, 10, 20),
            Row("iam.users", "UserService", "services.iam", "/admin/users", Permissions.Users.Read, 20, 20),
        };

        var result = NavigationMenuTreeBuilder.Build(rows, [Permissions.Stock.Read]);

        var group = Assert.Single(result.Groups);
        Assert.Equal("UserService", group.ServiceName);
        Assert.Equal("iam.security", Assert.Single(group.Items).Code);
    }

    [Fact]
    public void Build_omits_service_group_when_all_items_filtered()
    {
        var rows = new[]
        {
            Row("stock.units", "StockService", "services.stock", "/units-of-measure", Permissions.Stock.Read, 10, 10),
        };

        var result = NavigationMenuTreeBuilder.Build(rows, [Permissions.Users.Read]);

        Assert.Empty(result.Groups);
    }

    [Fact]
    public void Build_orders_groups_by_service_display_order()
    {
        var rows = new[]
        {
            Row("stock.units", "StockService", "services.stock", "/units-of-measure", null, 10, 10),
            Row("general.dashboard", "General", "services.general", "/", null, 10, 0),
            Row("iam.security", "UserService", "services.iam", "/account/security", null, 10, 20),
        };

        var result = NavigationMenuTreeBuilder.Build(rows, []);

        Assert.Equal(
            ["General", "StockService", "UserService"],
            result.Groups.Select(g => g.ServiceName).ToArray()
        );
    }

    [Fact]
    public void Build_nests_children_under_parent()
    {
        var parentId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1");
        var childId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2");
        var rows = new[]
        {
            new NavigationMenuRow(
                parentId,
                "stock.root",
                "StockService",
                "services.stock",
                null,
                "/stock",
                "warehouse",
                "menu.products",
                null,
                10,
                10
            ),
            new NavigationMenuRow(
                childId,
                "stock.units",
                "StockService",
                "services.stock",
                parentId,
                "/units-of-measure",
                "units",
                "menu.unitsOfMeasure",
                null,
                10,
                10
            ),
        };

        var result = NavigationMenuTreeBuilder.Build(rows, []);
        var item = Assert.Single(Assert.Single(result.Groups).Items);
        Assert.Equal("stock.root", item.Code);
        Assert.Equal("stock.units", Assert.Single(item.Children!).Code);
    }

    private static NavigationMenuRow Row(
        string code,
        string service,
        string serviceKey,
        string route,
        string? permission,
        int displayOrder,
        int serviceOrder
    ) =>
        new(
            Guid.NewGuid(),
            code,
            service,
            serviceKey,
            null,
            route,
            "icon",
            "menu.x",
            permission,
            displayOrder,
            serviceOrder
        );
}
