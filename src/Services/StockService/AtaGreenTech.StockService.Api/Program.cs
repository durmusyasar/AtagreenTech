using AtaGreenTech.StockService.Infrastructure;
using OmniSync.Core.Infrastructure.Cors;
using OmniSync.Core.Infrastructure.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.AddOmniSyncCore(options =>
{
    options.AddApplicationAssemblies(typeof(Program).Assembly)
        .AddStockInfrastructure()
        .AddOmniSyncCors();
});

var app = builder.Build();

// `dotnet AtaGreenTech.StockService.Api.dll migrate-tenants`: tüm aktif tenant veritabanlarını migrate edip çıkar.
if (args is ["migrate-tenants"])
{
    var summary = await app.MigrateTenantDatabasesAsync();
    return summary.HasFailures ? 1 : 0;
}

// Routing, tenant çözümlemesi, CORS, authentication ve authorization burada sırasıyla eklenir;
// UseCors/UseAuthentication/UseAuthorization ayrıca çağrılmamalıdır.
app.UseOmniSyncCore();
app.MapOmniSyncEndpoints();

await app.RunAsync();
return 0;
