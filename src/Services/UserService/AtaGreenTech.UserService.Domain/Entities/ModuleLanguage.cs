namespace AtaGreenTech.UserService.Domain.Entities;

/// <summary>Modül adı çevirisi (ModuleId + LanguageId unique).</summary>
public sealed class ModuleLanguage
{
    public const int NameMaxLength = 128;

    public Guid Id { get; init; }
    public Guid ModuleId { get; init; }
    public Guid LanguageId { get; init; }
    public string Name { get; init; } = null!;
}
