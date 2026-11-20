namespace AtaGreenTech.UserService.Domain.Entities
{
    /// <summary>
    /// Master dil kataloğu şekli (<c>tr</c>, <c>en</c>, …).
    /// EF aggregate değildir; satırlar SQL ile yönetilir. Uzunluk sabitleri şema sözleşmesidir.
    /// </summary>
    public sealed class Language
    {
        public const int CodeMaxLength = 16;
        public const int NameMaxLength = 64;

        public Guid Id { get; init; }
        public string Code { get; init; } = null!;
        public string Name { get; init; } = null!;
        public bool IsDefault { get; init; }
        public bool IsActive { get; init; } = true;
    }
}