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
            Row("iam.security", "IAM", "UserService", "/account/security", null, 10, 20),
            Row("iam.users", "IAM", "UserService", "/admin/users", Permissions.Users.Read, 20, 20),
        };

        var result = NavigationMenuTreeBuilder.Build(rows, [Permissions.Stock.Read], "tr", "tr");

        var group = Assert.Single(result.Groups);
        Assert.Equal("UserService", group.ServiceName);
        Assert.Equal("iam.security", Assert.Single(group.Items).Code);
    }

    [Fact]
    public void Build_omits_service_group_when_all_items_filtered()
    {
        var rows = new[]
        {
            Row("stock.units", "STOCK", "StockService", "/units-of-measure", Permissions.Stock.Read, 10, 10),
        };

        var result = NavigationMenuTreeBuilder.Build(rows, [Permissions.Users.Read], "tr", "tr");

        Assert.Empty(result.Groups);
    }

    [Fact]
    public void Build_orders_groups_by_module_display_order()
    {
        var rows = new[]
        {
            Row("stock.units", "STOCK", "StockService", "/units-of-measure", null, 10, 10),
            Row("general.dashboard", "GENERAL", "General", "/", null, 10, 0),
            Row("iam.security", "IAM", "UserService", "/account/security", null, 10, 20),
        };

        var result = NavigationMenuTreeBuilder.Build(rows, [], "tr", "tr");

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
                "STOCK",
                "StockService",
                null,
                "/stock",
                "warehouse",
                null,
                10,
                10,
                Map(("tr", "Stok"), ("en", "Stock")),
                Map(("tr", "Ürünler"), ("en", "Products"))
            ),
            new NavigationMenuRow(
                childId,
                "stock.units",
                "STOCK",
                "StockService",
                parentId,
                "/units-of-measure",
                "units",
                null,
                10,
                10,
                Map(("tr", "Stok"), ("en", "Stock")),
                Map(("tr", "Ölçü"), ("en", "Units"))
            ),
        };

        var result = NavigationMenuTreeBuilder.Build(rows, [], "tr", "tr");
        var item = Assert.Single(Assert.Single(result.Groups).Items);
        Assert.Equal("stock.root", item.Code);
        Assert.Equal("Ürünler", item.Title);
        Assert.Equal("stock.units", Assert.Single(item.Children!).Code);
    }

    [Fact]
    public void Build_falls_back_to_default_language_then_code()
    {
        var rows = new[]
        {
            new NavigationMenuRow(
                Guid.NewGuid(),
                "stock.units",
                "STOCK",
                "StockService",
                null,
                "/units-of-measure",
                "units",
                null,
                10,
                10,
                Map(("tr", "Stok")),
                Map(("tr", "Ölçü Birimleri"))
            ),
        };

        var result = NavigationMenuTreeBuilder.Build(rows, [], "en", "tr");
        var group = Assert.Single(result.Groups);
        Assert.Equal("Stok", group.Title);
        Assert.Equal("Ölçü Birimleri", Assert.Single(group.Items).Title);
    }

    [Fact]
    public void Build_stops_on_parent_cycle_without_throwing()
    {
        var a = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1");
        var b = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2");
        var rows = new[]
        {
            new NavigationMenuRow(
                a,
                "stock.a",
                "STOCK",
                "StockService",
                b,
                "/a",
                "a",
                null,
                10,
                10,
                Map(("tr", "Stok")),
                Map(("tr", "A"))
            ),
            new NavigationMenuRow(
                b,
                "stock.b",
                "STOCK",
                "StockService",
                a,
                "/b",
                "b",
                null,
                20,
                10,
                Map(("tr", "Stok")),
                Map(("tr", "B"))
            ),
        };

        var result = NavigationMenuTreeBuilder.Build(rows, [], "tr", "tr");
        Assert.Empty(result.Groups);
    }

    private static NavigationMenuRow Row(
        string code,
        string moduleCode,
        string service,
        string route,
        string? permission,
        int displayOrder,
        int moduleOrder
    ) =>
        new(
            Guid.NewGuid(),
            code,
            moduleCode,
            service,
            null,
            route,
            "icon",
            permission,
            displayOrder,
            moduleOrder,
            Map(("tr", moduleCode), ("en", moduleCode)),
            Map(("tr", code), ("en", code))
        );

    private static IReadOnlyDictionary<string, string> Map(params (string Code, string Name)[] pairs) =>
        pairs.ToDictionary(p => p.Code, p => p.Name, StringComparer.OrdinalIgnoreCase);
}
