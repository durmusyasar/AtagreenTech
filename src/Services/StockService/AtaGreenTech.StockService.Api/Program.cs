using AtaGreenTech.StockService.Api.Hosting;
using System.Text.Json.Serialization;
using AtaGreenTech.StockService.Api.Services;
using AtaGreenTech.StockService.Application.Interfaces;
using AtaGreenTech.StockService.Infrastructure;
using Microsoft.EntityFrameworkCore;
using OmniSync.Core.Infrastructure.Cors;
using OmniSync.Core.Infrastructure.Hosting;
using OmniSync.Core.Infrastructure.Persistence;

// `dotnet AtaGreenTech.StockService.Api.dll healthcheck`: container sağlık kontrolü; host oluşturulmadan çalışır.
if (args is [ContainerHealthProbe.Command])
    return await ContainerHealthProbe.RunAsync();

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

// `migrate-master` ve `migrate-tenants` komutları migration'ları uygulayıp çıkar; web sunucusu başlamaz ve JWT
// ayarları gerekmez. Birden çok replikada migration'ların aynı anda uygulanmaması için API'den önce ayrı çalışırlar.
if (args is ["migrate-master"])
{
    await using var scope = app.Services.CreateAsyncScope();
    await scope.ServiceProvider.GetRequiredService<MasterDbContext>().Database.MigrateAsync();
    return 0;
}

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
