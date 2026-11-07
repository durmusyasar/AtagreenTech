using System.Text.Json.Serialization;
using AtaGreenTech.StockService.Api.Services;
using AtaGreenTech.StockService.Application.Interfaces;
using AtaGreenTech.StockService.Infrastructure;
using OmniSync.Core.Infrastructure.Cors;
using OmniSync.Core.Infrastructure.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Enum'lar (ör. hareket tipi) yanıtlarda adıyla yazılır; isteklerde hem ad hem sayı kabul edilir.
builder.Services.ConfigureHttpJsonOptions(json => json.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddScoped<IRequestOriginProvider, HttpRequestOriginProvider>();

builder.AddOmniSyncCore(options =>
{
    // Endpoint modülleri Api'den, handler ve validator'lar Application'dan taranır.
    options.AddApplicationAssemblies(typeof(Program).Assembly, typeof(IStockDbContext).Assembly)
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
