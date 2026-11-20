namespace AtaGreenTech.UserService.Domain.Entities
{
    /// <summary>
    /// Master DB uygulama modülü şekli (GENERAL / STOCK / IAM / SYSTEM).
    /// EF aggregate değildir; satırlar SQL ile yönetilir. Uzunluk sabitleri Infrastructure
    /// (ör. <c>SystemNavigationAdmin</c>) ve NotFoundException adları için tek kaynaktır.
    /// </summary>
    public sealed class Module
    {
        public const int CodeMaxLength = 32;
        public const int ServiceNameMaxLength = 64;
        public const int IconKeyMaxLength = 64;

        public Guid Id { get; init; }

        /// <summary>Modül kodu: GENERAL, STOCK, IAM, SYSTEM.</summary>
        public string Code { get; init; } = null!;

        /// <summary>Tenant abonelik anahtarı: General, StockService, UserService, System.</summary>
        public string ServiceName { get; init; } = null!;

        public string IconKey { get; init; } = null!;
        public int DisplayOrder { get; init; }
        public bool IsCore { get; init; }
        public bool IsActive { get; init; } = true;
    }
}