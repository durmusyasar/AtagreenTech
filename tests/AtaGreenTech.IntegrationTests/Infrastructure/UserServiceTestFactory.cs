extern alias UserApi;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace AtaGreenTech.IntegrationTests.Infrastructure;

/// <summary>UserService API'yi paylaşılan JWT + MasterDb ile ayağa kaldırır.</summary>
public sealed class UserServiceTestFactory : WebApplicationFactory<UserApi::Program>
{
    private readonly string _masterConnectionString;

    public UserServiceTestFactory(string masterConnectionString)
    {
        _masterConnectionString = masterConnectionString;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        // UseSetting, appsettings.Development.json'daki localhost MasterDb'yi ezer.
        builder.UseSetting("ConnectionStrings:MasterDb", _masterConnectionString);
        builder.UseSetting("JwtSettings:SigningKey", SharedJwtContract.SigningKey);
        builder.UseSetting("JwtSettings:Issuer", SharedJwtContract.Issuer);
        builder.UseSetting("JwtSettings:Audience", SharedJwtContract.Audience);
        builder.UseSetting("Auth:Bootstrap:Password", SharedJwtContract.AdminPassword);
        builder.UseSetting("Auth:AuthRateLimitPermitLimit", "1000");
        builder.UseSetting("Auth:PublicAppBaseUrl", "http://localhost");
        builder.UseSetting("TenantResolution:RootDomains:0", "omnisync.localhost");
        builder.UseSetting("TenantMigrations:RunOnStartup", "false");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["ConnectionStrings:MasterDb"] = _masterConnectionString,
                    ["JwtSettings:SigningKey"] = SharedJwtContract.SigningKey,
                    ["JwtSettings:Issuer"] = SharedJwtContract.Issuer,
                    ["JwtSettings:Audience"] = SharedJwtContract.Audience,
                    ["Auth:Bootstrap:Password"] = SharedJwtContract.AdminPassword,
                    ["Auth:AuthRateLimitPermitLimit"] = "1000",
                    ["Auth:PublicAppBaseUrl"] = "http://localhost",
                    ["TenantResolution:RootDomains:0"] = "omnisync.localhost",
                    ["TenantMigrations:RunOnStartup"] = "false",
                    ["Cors:AllowedOrigins:0"] = "http://localhost",
                }
            );
        });
    }
}
