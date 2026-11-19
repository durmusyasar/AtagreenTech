namespace AtaGreenTech.UserService.Domain.Entities;

/// <summary>Master DB dil kataloğu (<c>tr</c>, <c>en</c>, …).</summary>
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
