namespace AtaGreenTech.UserService.Domain.Entities
{
    /// <summary>
    /// Master menü adı çevirisi şekli (MenuId + LanguageId unique).
    /// EF aggregate değildir; satırlar SQL ile yönetilir. <see cref="NameMaxLength"/>
    /// Infrastructure normalizasyonunda kullanılır.
    /// </summary>
    public sealed class MenuLanguage
    {
        public const int NameMaxLength = 128;

        public Guid Id { get; init; }
        public Guid MenuId { get; init; }
        public Guid LanguageId { get; init; }
        public string Name { get; init; } = null!;
    }
}