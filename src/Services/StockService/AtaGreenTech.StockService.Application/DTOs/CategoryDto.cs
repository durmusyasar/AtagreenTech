namespace AtaGreenTech.StockService.Application.DTOs;

/// <param name="Name">İstenen dile göre çözümlenen ad.</param>
/// <param name="Description">İstenen dile göre çözümlenen açıklama.</param>
/// <param name="ParentCategoryName">Üst kategorinin istenen dile göre çözümlenen adı.</param>
public sealed record CategoryDto(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    Guid? ParentCategoryId,
    string? ParentCategoryName,
    bool IsActive,
    DateTime CreatedAt,
    IReadOnlyList<CategoryTranslationDto> Translations
);
