using AtaGreenTech.UserService.Application.Features.Auth;
using AtaGreenTech.UserService.Domain.Entities;

namespace AtaGreenTech.UserService.Tests.Application;

public sealed class UserMeResponseTests
{
    [Fact]
    public void Response_exposes_profile_fields_without_tenant_column_on_user_entity()
    {
        var tenantId = Guid.Parse("5803ca09-ca89-4f92-81ce-66a0477cb081");
        var me = new UserMeResponse(
            Guid.NewGuid(),
            "admin@omnisync.com",
            "admin",
            "System",
            "Admin",
            tenantId.ToString(),
            ["Admin"],
            ["users.read", "stock.read"]
        );

        Assert.Equal("admin@omnisync.com", me.Email);
        Assert.Equal(tenantId.ToString(), me.TenantId);
        Assert.Equal(["Admin"], me.Roles);
        Assert.Equal(["users.read", "stock.read"], me.Permissions);
        Assert.DoesNotContain("TenantId", typeof(User).GetProperties().Select(p => p.Name));
    }
}
