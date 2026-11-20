using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace AtaGreenTech.SharedKernel.Security;

/// <summary>
/// Master DB denylist + kısa TTL bellek önbelleği. UserService ve StockService aynı Postgres master'ı paylaşır.
/// </summary>
public sealed class MasterAccessTokenRevocationStore(
    DbContext master,
    IMemoryCache cache
) : IAccessTokenRevocationStore
{
    private static readonly TimeSpan NegativeCacheTtl = TimeSpan.FromSeconds(15);

    public Task RevokeSessionAsync(Guid sessionId, TimeSpan ttl, CancellationToken cancellationToken = default) =>
        RevokeKeyAsync(SessionKey(sessionId), ttl, cancellationToken);

    public Task RevokeTokenAsync(string jti, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(jti);
        return RevokeKeyAsync(TokenKey(jti), ttl, cancellationToken);
    }

    public Task<bool> IsSessionRevokedAsync(Guid sessionId, CancellationToken cancellationToken = default) =>
        IsRevokedAsync(SessionKey(sessionId), cancellationToken);

    public Task<bool> IsTokenRevokedAsync(string jti, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(jti);
        return IsRevokedAsync(TokenKey(jti), cancellationToken);
    }

    private async Task RevokeKeyAsync(string key, TimeSpan ttl, CancellationToken cancellationToken)
    {
        if (ttl <= TimeSpan.Zero)
            ttl = TimeSpan.FromMinutes(JwtContract.DefaultAccessTokenMinutes);

        var expires = DateTime.UtcNow.Add(ttl);
        await master.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO "AccessTokenRevocations" ("Key", "ExpiresAtUtc")
            VALUES ({0}, {1})
            ON CONFLICT ("Key") DO UPDATE SET "ExpiresAtUtc" = EXCLUDED."ExpiresAtUtc"
            """,
            [key, expires],
            cancellationToken
        );

        cache.Set(CacheKey(key), true, new MemoryCacheEntryOptions { AbsoluteExpiration = expires });
    }

    private async Task<bool> IsRevokedAsync(string key, CancellationToken cancellationToken)
    {
        if (cache.TryGetValue(CacheKey(key), out bool cached))
            return cached;

        var now = DateTime.UtcNow;
        var rows = await master.Database.SqlQueryRaw<RevocationRow>(
            """
            SELECT "ExpiresAtUtc" AS "ExpiresAtUtc"
            FROM "AccessTokenRevocations"
            WHERE "Key" = {0} AND "ExpiresAtUtc" > {1}
            LIMIT 1
            """,
            key,
            now
        ).ToListAsync(cancellationToken);

        var revoked = rows.Count > 0;
        if (revoked)
        {
            var expires = rows[0].ExpiresAtUtc;
            cache.Set(
                CacheKey(key),
                true,
                new MemoryCacheEntryOptions { AbsoluteExpiration = expires }
            );
        }
        else
        {
            cache.Set(CacheKey(key), false, NegativeCacheTtl);
        }

        return revoked;
    }

    private static string SessionKey(Guid sessionId) => $"sid:{sessionId:D}";

    private static string TokenKey(string jti) => $"jti:{jti.Trim()}";

    private static string CacheKey(string key) => $"atr:{key}";

    private sealed class RevocationRow
    {
        public DateTime ExpiresAtUtc { get; init; }
    }
}
