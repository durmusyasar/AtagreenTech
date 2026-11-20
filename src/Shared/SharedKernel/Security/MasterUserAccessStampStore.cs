using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace AtaGreenTech.SharedKernel.Security;

/// <summary>Master <c>UserAccessStamps</c> + bellek önbelleği (#70 Stock parity).</summary>
public sealed class MasterUserAccessStampStore(DbContext master, IMemoryCache cache) : IUserAccessStampStore
{
    private static readonly TimeSpan PositiveCacheTtl = TimeSpan.FromSeconds(30);

    public async Task PublishAsync(
        Guid tenantId,
        Guid userId,
        string securityStamp,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(securityStamp);
        var stamp = securityStamp.Trim();
        var now = DateTime.UtcNow;

        await master.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO "UserAccessStamps" ("TenantId", "UserId", "SecurityStamp", "UpdatedAtUtc")
            VALUES ({0}, {1}, {2}, {3})
            ON CONFLICT ("TenantId", "UserId") DO UPDATE
            SET "SecurityStamp" = EXCLUDED."SecurityStamp",
                "UpdatedAtUtc" = EXCLUDED."UpdatedAtUtc"
            """,
            [tenantId, userId, stamp, now],
            cancellationToken
        );

        cache.Set(CacheKey(tenantId, userId), stamp, PositiveCacheTtl);
    }

    public async Task<string?> GetAsync(
        Guid tenantId,
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        if (cache.TryGetValue(CacheKey(tenantId, userId), out string? cached))
            return cached;

        var rows = await master.Database.SqlQueryRaw<StampRow>(
            """
            SELECT "SecurityStamp" AS "SecurityStamp"
            FROM "UserAccessStamps"
            WHERE "TenantId" = {0} AND "UserId" = {1}
            LIMIT 1
            """,
            tenantId,
            userId
        ).ToListAsync(cancellationToken);

        var stamp = rows.Count > 0 ? rows[0].SecurityStamp : null;
        if (stamp is not null)
            cache.Set(CacheKey(tenantId, userId), stamp, PositiveCacheTtl);

        return stamp;
    }

    private static string CacheKey(Guid tenantId, Guid userId) => $"uas:{tenantId:D}:{userId:D}";

    private sealed class StampRow
    {
        public string SecurityStamp { get; init; } = null!;
    }
}
