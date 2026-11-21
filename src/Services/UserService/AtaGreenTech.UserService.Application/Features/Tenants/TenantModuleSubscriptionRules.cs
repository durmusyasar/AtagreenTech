namespace AtaGreenTech.UserService.Application.Features.Tenants;

/// <summary>
/// Tenant modül aboneliği aktiflik kuralları (#66/#67).
/// Nav / Stock checker ile aynı: core her zaman aktif; non-core → enabled ve
/// (<c>ExpiresAtUtc</c> yok veya <c>ExpiresAtUtc &gt; utcNow</c>).
/// </summary>
public static class TenantModuleSubscriptionRules
{
    /// <summary>Modül fiilen erişilebilir mi (runtime + liste özeti).</summary>
    public static bool IsEffectivelyActive(
        bool isCore,
        bool isEnabled,
        DateTime? expiresAtUtc,
        DateTime utcNow
    )
    {
        if (isCore)
            return true;
        if (!isEnabled)
            return false;
        return expiresAtUtc is null || expiresAtUtc > utcNow;
    }

    /// <summary>
    /// Non-core, açık ama süresi dolmuş (aktif sayıya dahil edilmez; uyarı rozeti).
    /// </summary>
    public static bool IsExpiredEnabled(
        bool isCore,
        bool isEnabled,
        DateTime? expiresAtUtc,
        DateTime utcNow
    )
    {
        if (isCore || !isEnabled || expiresAtUtc is null)
            return false;
        return expiresAtUtc <= utcNow;
    }
}
