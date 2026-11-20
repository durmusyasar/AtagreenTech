using AtaGreenTech.UserService.Api.Hosting;
using System.Text.Json.Serialization;
using AtaGreenTech.SharedKernel.Localization;
using AtaGreenTech.SharedKernel.Security;
using AtaGreenTech.UserService.Application;
using AtaGreenTech.UserService.Infrastructure;
using AtaGreenTech.UserService.Infrastructure.Persistence.Master;
using Microsoft.EntityFrameworkCore;
using OmniSync.Core.Infrastructure.Cors;
using OmniSync.Core.Infrastructure.Hosting;
using OmniSync.Core.Infrastructure.Persistence;

// `dotnet AtaGreenTech.UserService.Api.dll healthcheck`: container sağlık kontrolü; host oluşturulmadan çalışır.
if (args is [ContainerHealthProbe.Command])
    return await ContainerHealthProbe.RunAsync();

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(json => json.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// #71: Business-rule 422 handler OmniSync GlobalExceptionHandler'dan önce kaydedilmeli.
builder.Services.AddAtaGreenLocalization();
builder.Services.AddExceptionHandler<LocalizedBusinessRuleExceptionHandler>();

builder.AddOmniSyncCore(options =>
{
    // Endpoint modülleri Api'den, handler ve validator'lar Application'dan taranır.
    options.AddApplicationAssemblies(typeof(Program).Assembly, typeof(AssemblyMarker).Assembly)
        .AddUserInfrastructure()
        .AddOmniSyncCors();
});

builder.Services.AddAuthRateLimiting(builder.Configuration);

var app = builder.Build();

// `migrate-master` ve `migrate-tenants` komutları migration'ları uygulayıp çıkar; web sunucusu başlamaz ve JWT
// ayarları gerekmez. Birden çok replikada migration'ların aynı anda uygulanmaması için API'den önce ayrı çalışırlar.
if (args is ["migrate-master"])
{
    await using var scope = app.Services.CreateAsyncScope();
    var master = scope.ServiceProvider.GetRequiredService<MasterDbContext>();
    await master.Database.MigrateAsync();
    // OmniSync Tenant modeline dokunmadan menü kataloğu + servis aboneliklerini master'a ekler.
    await MasterNavigationSchema.EnsureAsync(master);
    await master.Database.EnsureAccessTokenRevocationSchemaAsync();
    return 0;
}

if (args is ["migrate-tenants"])
{
    var summary = await app.MigrateTenantDatabasesAsync();
    return summary.HasFailures ? 1 : 0;
}

// `seed-tenant <subdomain> [--email=...] [--password=...]`: RBAC + varsayılan Admin (add-tenant otomasyonu).
if (args is [TenantSeedCli.Command, ..])
    return await TenantSeedCli.RunAsync(app, args);

// #71: Culture, OmniSync auth/tenant/endpoint hattından önce set edilmeli (401/403 mesaj dili).
app.UseRequestLocalization();

// Routing, tenant çözümlemesi, CORS, authentication ve authorization burada sırasıyla eklenir;
// UseCors/UseAuthentication/UseAuthorization ayrıca çağrılmamalıdır.
app.UseOmniSyncCore();
app.UseRateLimiter();
app.MapOmniSyncEndpoints();

PermissionPolicyStartupAudit.LogRegisteredPolicies(
    app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("PermissionPolicies")
);

await app.RunAsync();
return 0;

// WebApplicationFactory<Program> için entry-point görünürlüğü.
public partial class Program;
