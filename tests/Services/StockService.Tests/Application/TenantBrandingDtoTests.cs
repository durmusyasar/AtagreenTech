using AtaGreenTech.StockService.Application.DTOs;

namespace AtaGreenTech.StockService.Tests.Application;

public sealed class TenantBrandingDtoTests
{
    [Fact]
    public void Record_carries_public_branding_fields_for_anonymous_config()
    {
        var tenantId = Guid.Parse("5803ca09-ca89-4f92-81ce-66a0477cb081");
        var dto = new TenantBrandingDto(
            tenantId,
            "atagreen",
            "AtaGreen",
            "#2e7d32",
            "/favicon.svg",
            "/favicon.svg"
        );

        Assert.Equal(tenantId, dto.TenantId);
        Assert.Equal("atagreen", dto.Subdomain);
        Assert.Equal("AtaGreen", dto.Name);
        Assert.Equal("#2e7d32", dto.PrimaryColor);
        Assert.Equal("/favicon.svg", dto.LogoUrl);
        Assert.Equal("/favicon.svg", dto.FaviconUrl);
    }
}
