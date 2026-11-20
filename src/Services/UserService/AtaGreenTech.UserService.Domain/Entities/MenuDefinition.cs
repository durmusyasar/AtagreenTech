namespace AtaGreenTech.UserService.Domain.Entities;

/// <summary>
/// Master DB menü kataloğu satırı. Tenant'tan bağımsız; görünürlük abonelik + izin ile filtrelenir.
/// Başlıklar <see cref="MenuLanguage"/> üzerinden gelir.
/// </summary>
public sealed class MenuDefinition
{
    public const int CodeMaxLength = 128;
    public const int RoutePathMaxLength = 256;
    public const int IconKeyMaxLength = 64;
    public const int RequiredPermissionMaxLength = 128;

    public Guid Id { get; init; }

    public Guid ModuleId { get; init; }

    /// <summary>Benzersiz menü kodu (ör. <c>stock.units</c>).</summary>
    public string Code { get; init; } = null!;

    public Guid? ParentId { get; init; }

    public string RoutePath { get; init; } = null!;

    public string IconKey { get; init; } = null!;

    /// <summary>Null ise yalnızca oturum açmış olmak yeterli.</summary>
    public string? RequiredPermission { get; init; }

    public int DisplayOrder { get; init; }

    public bool IsActive { get; init; } = true;
}
