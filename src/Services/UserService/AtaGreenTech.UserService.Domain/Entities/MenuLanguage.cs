namespace AtaGreenTech.UserService.Domain.Entities;

/// <summary>Menü adı çevirisi (MenuId + LanguageId unique).</summary>
public sealed class MenuLanguage
{
    public const int NameMaxLength = 128;

    public Guid Id { get; init; }
    public Guid MenuId { get; init; }
    public Guid LanguageId { get; init; }
    public string Name { get; init; } = null!;
}
