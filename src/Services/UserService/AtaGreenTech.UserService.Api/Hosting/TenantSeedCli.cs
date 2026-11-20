using AtaGreenTech.UserService.Application.Features.Auth;
using AtaGreenTech.UserService.Application.Interfaces;
using OmniSync.Core.Domain.Exceptions;
using OmniSync.Core.Domain.Tenants;

namespace AtaGreenTech.UserService.Api.Hosting;

/// <summary>
/// <c>seed-tenant &lt;subdomain&gt;</c> CLI: RBAC + varsayılan Admin'i tenant kapsamında idempotent seed eder.
/// </summary>
public static class TenantSeedCli
{
    public const string Command = "seed-tenant";

    public static async Task<int> RunAsync(WebApplication app, string[] args)
    {
        if (args.Length < 2 || string.IsNullOrWhiteSpace(args[1]))
        {
            Console.Error.WriteLine(
                "Kullanım: seed-tenant <subdomain> [--email=...] [--password=...] [--username=...] [--first-name=...] [--last-name=...]"
            );
            return 64;
        }

        var subdomain = args[1].Trim().ToLowerInvariant();
        var request = new TenantIdentitySeedRequest(
            AdminEmail: GetArgValue(args, "--email"),
            AdminPassword: GetArgValue(args, "--password"),
            AdminUsername: GetArgValue(args, "--username"),
            AdminFirstName: GetArgValue(args, "--first-name"),
            AdminLastName: GetArgValue(args, "--last-name")
        );

        try
        {
            var tenants = app.Services.GetRequiredService<ITenantExecutionContext>();

            // OmniSync generic overload: sonuç scope dışına güvenle döner (closure + null-forgiving yok).
            var result = await tenants.RunInTenantScopeAsync(
                subdomain,
                async (sp, ct) =>
                {
                    var seeder = sp.GetRequiredService<ITenantIdentitySeeder>();
                    return await seeder.EnsureAsync(
                        request,
                        TenantIdentitySeedMode.UpsertAdmin,
                        ct
                    );
                },
                CancellationToken.None
            );

            Console.WriteLine(
                $"[SEED] Tenant={result.TenantIdentifier} | Admin={result.AdminEmail} | Created={result.AdminCreated} | UserId={result.AdminUserId}"
            );
            return 0;
        }
        catch (ArgumentException ex)
        {
            Console.Error.WriteLine($"[SEED] Geçersiz girdi: {ex.Message}");
            return 65;
        }
        catch (NotFoundException ex)
        {
            Console.Error.WriteLine($"[SEED] Tenant bulunamadı: {ex.Message}");
            return 66;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[SEED] Hata: {ex.Message}");
            return 1;
        }
    }

    private static string? GetArgValue(string[] args, string prefix) =>
        args.FirstOrDefault(a => a.StartsWith($"{prefix}=", StringComparison.OrdinalIgnoreCase))
            ?.Split('=', 2)[1];
}
