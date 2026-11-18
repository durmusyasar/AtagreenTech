namespace AtaGreenTech.SharedKernel.Security;

/// <summary>
/// ASP.NET authorization policy adları; claim tipi <see cref="JwtContract.Claims.Permission"/>.
/// </summary>
public static class AuthorizationPolicies
{
    public const string UsersRead = "perm:users.read";
    public const string UsersWrite = "perm:users.write";
    public const string UsersDelete = "perm:users.delete";
    public const string AuditRead = "perm:audit.read";
    public const string SystemRead = "perm:system.read";
    public const string StockRead = "perm:stock.read";
    public const string StockWrite = "perm:stock.write";
    public const string StockDelete = "perm:stock.delete";

    /// <summary>Policy adı → tek permission claim değeri.</summary>
    public static IReadOnlyList<(string PolicyName, string Permission)> Bindings { get; } =
    [
        (UsersRead, Permissions.Users.Read),
        (UsersWrite, Permissions.Users.Write),
        (UsersDelete, Permissions.Users.Delete),
        (AuditRead, Permissions.Audit.Read),
        (SystemRead, Permissions.System.Read),
        (StockRead, Permissions.Stock.Read),
        (StockWrite, Permissions.Stock.Write),
        (StockDelete, Permissions.Stock.Delete),
    ];
}
