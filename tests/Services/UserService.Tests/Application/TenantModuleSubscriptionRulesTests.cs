using AtaGreenTech.UserService.Application.Features.Tenants;

namespace AtaGreenTech.UserService.Tests.Application;

public sealed class TenantModuleSubscriptionRulesTests
{
    private static readonly DateTime UtcNow = new(2026, 11, 21, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void IsEffectivelyActive_core_always_true_even_when_disabled()
    {
        Assert.True(
            TenantModuleSubscriptionRules.IsEffectivelyActive(
                isCore: true,
                isEnabled: false,
                expiresAtUtc: UtcNow.AddDays(-1),
                utcNow: UtcNow
            )
        );
    }

    [Fact]
    public void IsEffectivelyActive_optional_requires_enabled()
    {
        Assert.False(
            TenantModuleSubscriptionRules.IsEffectivelyActive(
                isCore: false,
                isEnabled: false,
                expiresAtUtc: null,
                utcNow: UtcNow
            )
        );
    }

    [Fact]
    public void IsEffectivelyActive_optional_null_expiry_is_active()
    {
        Assert.True(
            TenantModuleSubscriptionRules.IsEffectivelyActive(
                isCore: false,
                isEnabled: true,
                expiresAtUtc: null,
                utcNow: UtcNow
            )
        );
    }

    [Fact]
    public void IsEffectivelyActive_optional_future_expiry_is_active()
    {
        Assert.True(
            TenantModuleSubscriptionRules.IsEffectivelyActive(
                isCore: false,
                isEnabled: true,
                expiresAtUtc: UtcNow.AddHours(1),
                utcNow: UtcNow
            )
        );
    }

    [Fact]
    public void IsEffectivelyActive_optional_past_expiry_is_inactive()
    {
        Assert.False(
            TenantModuleSubscriptionRules.IsEffectivelyActive(
                isCore: false,
                isEnabled: true,
                expiresAtUtc: UtcNow.AddSeconds(-1),
                utcNow: UtcNow
            )
        );
    }

    [Fact]
    public void IsExpiredEnabled_only_when_optional_enabled_and_past()
    {
        Assert.True(
            TenantModuleSubscriptionRules.IsExpiredEnabled(
                isCore: false,
                isEnabled: true,
                expiresAtUtc: UtcNow,
                utcNow: UtcNow
            )
        );
        Assert.False(
            TenantModuleSubscriptionRules.IsExpiredEnabled(
                isCore: true,
                isEnabled: true,
                expiresAtUtc: UtcNow.AddDays(-1),
                utcNow: UtcNow
            )
        );
        Assert.False(
            TenantModuleSubscriptionRules.IsExpiredEnabled(
                isCore: false,
                isEnabled: false,
                expiresAtUtc: UtcNow.AddDays(-1),
                utcNow: UtcNow
            )
        );
        Assert.False(
            TenantModuleSubscriptionRules.IsExpiredEnabled(
                isCore: false,
                isEnabled: true,
                expiresAtUtc: null,
                utcNow: UtcNow
            )
        );
    }
}
