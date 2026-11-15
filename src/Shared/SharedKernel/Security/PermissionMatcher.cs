namespace AtaGreenTech.SharedKernel.Security;

/// <summary>
/// JWT <c>permission</c> claim değerlerinin istenen izni karşılayıp karşılamadığını kontrol eder.
/// Somut izin (<c>stock.read</c>), alan jokerı (<c>stock.*</c>) ve sistem jokerı (<c>*</c>) desteklenir.
/// </summary>
public static class PermissionMatcher
{
    public static bool Satisfies(IEnumerable<string> granted, string required)
    {
        ArgumentNullException.ThrowIfNull(granted);
        ArgumentException.ThrowIfNullOrWhiteSpace(required);

        var requiredNormalized = required.Trim();
        foreach (var raw in granted)
        {
            if (string.IsNullOrWhiteSpace(raw))
                continue;

            var grant = raw.Trim();
            if (grant.Equals(Permissions.SystemAll, StringComparison.OrdinalIgnoreCase))
                return true;

            if (grant.Equals(requiredNormalized, StringComparison.OrdinalIgnoreCase))
                return true;

            if (grant.EndsWith(".*", StringComparison.Ordinal)
                && requiredNormalized.StartsWith(grant[..^1], StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    public static bool HasPermission(System.Security.Claims.ClaimsPrincipal user, string required)
    {
        ArgumentNullException.ThrowIfNull(user);
        var granted = user.FindAll(JwtContract.Claims.Permission).Select(c => c.Value);
        return Satisfies(granted, required);
    }
}
