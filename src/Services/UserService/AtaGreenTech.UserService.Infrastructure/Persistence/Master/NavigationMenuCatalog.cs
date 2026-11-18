using AtaGreenTech.UserService.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using OmniSync.Core.Infrastructure.Persistence;

namespace AtaGreenTech.UserService.Infrastructure.Persistence.Master;

internal sealed class NavigationMenuCatalog(MasterDbContext master) : INavigationMenuCatalog
{
    public async Task<IReadOnlyList<NavigationMenuRow>> GetActiveMenuRowsAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default
    )
    {
        // Tenant'ta abonelik yoksa varsayılanları ekle (yeni tenant / migrate sırası).
        await master.Database.ExecuteSqlInterpolatedAsync(
            $"""
            INSERT INTO "TenantServiceSubscriptions" ("Id", "TenantId", "ServiceName", "IsEnabled", "SubscribedAtUtc")
            SELECT gen_random_uuid(), {tenantId}, s."ServiceName", s."IsEnabled", now()
            FROM (VALUES
                ('General', TRUE),
                ('UserService', TRUE),
                ('StockService', TRUE)
            ) AS s("ServiceName", "IsEnabled")
            WHERE NOT EXISTS (
                SELECT 1 FROM "TenantServiceSubscriptions" x
                WHERE x."TenantId" = {tenantId} AND x."ServiceName" = s."ServiceName"
            );
            """,
            cancellationToken
        );

        return await master.Database
            .SqlQueryRaw<NavigationMenuRowDto>(
                """
                SELECT m."Id", m."Code", m."ServiceName", m."ServiceTranslationKey", m."ParentId",
                       m."RoutePath", m."IconKey", m."TranslationKey", m."RequiredPermission",
                       m."DisplayOrder", m."ServiceDisplayOrder"
                FROM "MenuDefinitions" m
                INNER JOIN "TenantServiceSubscriptions" s
                    ON s."ServiceName" = m."ServiceName"
                   AND s."TenantId" = {0}
                   AND s."IsEnabled" = TRUE
                WHERE m."IsActive" = TRUE
                ORDER BY m."ServiceDisplayOrder", m."DisplayOrder", m."Code"
                """,
                tenantId
            )
            .Select(r => new NavigationMenuRow(
                r.Id,
                r.Code,
                r.ServiceName,
                r.ServiceTranslationKey,
                r.ParentId,
                r.RoutePath,
                r.IconKey,
                r.TranslationKey,
                r.RequiredPermission,
                r.DisplayOrder,
                r.ServiceDisplayOrder
            ))
            .ToListAsync(cancellationToken);
    }

    private sealed class NavigationMenuRowDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = null!;
        public string ServiceName { get; set; } = null!;
        public string ServiceTranslationKey { get; set; } = null!;
        public Guid? ParentId { get; set; }
        public string RoutePath { get; set; } = null!;
        public string IconKey { get; set; } = null!;
        public string TranslationKey { get; set; } = null!;
        public string? RequiredPermission { get; set; }
        public int DisplayOrder { get; set; }
        public int ServiceDisplayOrder { get; set; }
    }
}
