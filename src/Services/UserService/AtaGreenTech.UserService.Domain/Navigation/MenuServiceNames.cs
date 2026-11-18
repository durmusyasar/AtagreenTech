namespace AtaGreenTech.UserService.Domain.Navigation;

/// <summary>Menü / abonelik servis kimlikleri (master seed ve filtre ile hizalı).</summary>
public static class MenuServiceNames
{
    public const string General = "General";
    public const string StockService = "StockService";
    public const string UserService = "UserService";
    public const string System = "System";

    /// <summary>Yeni tenant için varsayılan açık modüller (Stock lisans ile kapatılabilir).</summary>
    public static IReadOnlyList<(string ServiceName, bool IsEnabled)> DefaultSubscriptions { get; } =
    [
        (General, true),
        (UserService, true),
        (StockService, true),
    ];
}
