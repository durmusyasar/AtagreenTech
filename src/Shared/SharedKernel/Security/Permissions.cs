namespace AtaGreenTech.SharedKernel.Security;

/// <summary>
/// Sistem genelinde kullanılan yetki anahtarları. JWT <c>permission</c> claim değerleri ve seed kataloğu ile hizalıdır.
/// </summary>
public static class Permissions
{
    public static class Users
    {
        public const string Read = "users.read";
        public const string Write = "users.write";
        public const string Delete = "users.delete";
    }

    public static class Audit
    {
        public const string Read = "audit.read";
    }

    public static class System
    {
        public const string Read = "system.read";
        public const string Manage = "system.manage";
    }

    public static class Stock
    {
        public const string Read = "stock.read";
        public const string Write = "stock.write";
        public const string Delete = "stock.delete";

        /// <summary>Dokümantasyon / geniş eşleme; seed ve JWT'de somut izinler kullanılır.</summary>
        public const string All = "stock.*";
    }

    /// <summary>Dokümantasyon; seed ve JWT'de somut izinler kullanılır.</summary>
    public const string SystemAll = "*";

    /// <summary>Veritabanına seed edilecek somut izinler (açıklamalarıyla).</summary>
    public static IReadOnlyList<(string Name, string Description)> Catalog { get; } =
    [
        (Users.Read, "Kullanıcıları okuma/listeleme"),
        (Users.Write, "Kullanıcı oluşturma/düzenleme"),
        (Users.Delete, "Kullanıcı silme/pasife alma"),
        (Audit.Read, "Kimlik denetim günlüğünü okuma"),
        (System.Read, "Sistem tanı ve bağlantı ekranını görüntüleme"),
        (System.Manage, "Sistem menü ve dil kataloğunu yönetme"),
        (Stock.Read, "Stok verilerini okuma"),
        (Stock.Write, "Stok ekleme/güncelleme"),
        (Stock.Delete, "Stok silme"),
    ];

    /// <summary>Varsayılan rol → izin eşlemeleri.</summary>
    public static IReadOnlyDictionary<string, string[]> DefaultRoleGrants { get; } =
        new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["Admin"] =
            [
                Users.Read,
                Users.Write,
                Users.Delete,
                Audit.Read,
                System.Read,
                System.Manage,
                Stock.Read,
                Stock.Write,
                Stock.Delete,
            ],
            ["Manager"] = [Users.Read, Stock.Read, Stock.Write],
            ["Viewer"] = [Users.Read, Stock.Read],
        };
}
