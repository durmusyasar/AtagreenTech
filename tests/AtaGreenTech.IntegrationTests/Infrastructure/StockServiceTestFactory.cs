extern alias StockApi;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace AtaGreenTech.IntegrationTests.Infrastructure;

/// <summary>StockService API'yi UserService ile aynı JWT sözleşmesi ve MasterDb ile ayağa kaldırır.</summary>
public sealed class StockServiceTestFactory : WebApplicationFactory<StockApi::Program>
{
    private readonly string _masterConnectionString;

    public StockServiceTestFactory(string masterConnectionString)
    {
        _masterConnectionString = masterConnectionString;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.UseSetting("ConnectionStrings:MasterDb", _masterConnectionString);
        builder.UseSetting("JwtSettings:SigningKey", SharedJwtContract.SigningKey);
        builder.UseSetting("JwtSettings:Issuer", SharedJwtContract.Issuer);
        builder.UseSetting("JwtSettings:Audience", SharedJwtContract.Audience);
        builder.UseSetting("TenantResolution:RootDomains:0", "omnisync.localhost");
        builder.UseSetting("TenantMigrations:RunOnStartup", "false");
        builder.UseSetting("Localization:DefaultLanguage", "tr");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["ConnectionStrings:MasterDb"] = _masterConnectionString,
                    ["JwtSettings:SigningKey"] = SharedJwtContract.SigningKey,
                    ["JwtSettings:Issuer"] = SharedJwtContract.Issuer,
                    ["JwtSettings:Audience"] = SharedJwtContract.Audience,
                    ["TenantResolution:RootDomains:0"] = "omnisync.localhost",
                    ["TenantMigrations:RunOnStartup"] = "false",
                    ["Cors:AllowedOrigins:0"] = "http://localhost",
                    ["Localization:DefaultLanguage"] = "tr",
                }
            );
        });
    }
}
