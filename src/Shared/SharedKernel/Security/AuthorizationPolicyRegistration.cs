using Microsoft.AspNetCore.Authorization;

namespace AtaGreenTech.SharedKernel.Security;

/// <summary>
/// UserService / StockService ortak permission politikalarını kaydeder (joker destekli).
/// </summary>
public static class AuthorizationPolicyRegistration
{
    /// <summary>Draft adlarıyla uyumlu Stock politikaları (perm:* ile aynı assertion).</summary>
    public const string StockReadPolicy = "StockReadPolicy";
    public const string StockWritePolicy = "StockWritePolicy";
    public const string StockDeletePolicy = "StockDeletePolicy";

    public static void AddAtaGreenPermissionPolicies(this AuthorizationOptions options)
    {
        foreach (var (policyName, permission) in AuthorizationPolicies.Bindings)
            options.AddPolicy(policyName, policy => RequirePermission(policy, permission));

        // Issue #34 taslak adları — mevcut perm:stock.* politikalarıyla eşdeğer.
        options.AddPolicy(StockReadPolicy, policy => RequirePermission(policy, Permissions.Stock.Read));
        options.AddPolicy(StockWritePolicy, policy => RequirePermission(policy, Permissions.Stock.Write));
        options.AddPolicy(StockDeletePolicy, policy => RequirePermission(policy, Permissions.Stock.Delete));
    }

    private static void RequirePermission(AuthorizationPolicyBuilder policy, string permission) =>
        policy.RequireAssertion(ctx => PermissionMatcher.HasPermission(ctx.User, permission));
}
