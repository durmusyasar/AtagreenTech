using AtaGreenTech.StockService.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using OmniSync.Core.Infrastructure.Persistence;

namespace AtaGreenTech.StockService.Infrastructure.Security;

/// <summary>
/// Master <c>Modules</c> / <c>TenantModuleSubscriptions</c> kontrolü.
/// Kısa TTL bellek önbelleği (MVP); UserService PUT sonrası cross-invalidate yok.
/// </summary>
internal sealed class ModuleSubscriptionChecker(
    MasterDbContext master,
    IMemoryCache cache
) : IModuleSubscriptionChecker
{
    internal static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(2);

    public async Task<bool> IsModuleActiveAsync(
        Guid tenantId,
        string moduleCode,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(moduleCode))
            return false;

        var code = moduleCode.Trim().ToUpperInvariant();
        var cacheKey = $"module-sub:{tenantId:D}:{code}";

        if (cache.TryGetValue(cacheKey, out bool cached))
            return cached;

        var active = await master.Database
            .SqlQueryRaw<FlagRow>(
                """
                SELECT (
                    EXISTS (
                        SELECT 1 FROM "Modules" m
                        WHERE m."Code" = {0}
                          AND m."IsActive" = TRUE
                          AND m."IsCore" = TRUE
                    )
                    OR EXISTS (
                        SELECT 1
                        FROM "TenantModuleSubscriptions" s
                        INNER JOIN "Modules" m ON m."Id" = s."ModuleId"
                        WHERE s."TenantId" = {1}
                          AND m."Code" = {0}
                          AND m."IsActive" = TRUE
                          AND s."IsEnabled" = TRUE
                          AND (s."ExpiresAtUtc" IS NULL OR s."ExpiresAtUtc" > now())
                    )
                ) AS "Value"
                """,
                code,
                tenantId
            )
            .Select(r => r.Value)
            .FirstAsync(cancellationToken);

        cache.Set(cacheKey, active, CacheTtl);
        return active;
    }

    private sealed class FlagRow
    {
        public bool Value { get; set; }
    }
}
