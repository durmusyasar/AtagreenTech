namespace AtaGreenTech.StockService.Application.DTOs;

/// <param name="Name">İstenen dile göre çözümlenen ad.</param>
/// <param name="Description">İstenen dile göre çözümlenen açıklama.</param>
/// <param name="Currency">Birim maliyetin ISO 4217 para birimi.</param>
/// <param name="CategoryName">Kategorinin istenen dile göre çözümlenen adı.</param>
/// <param name="Translations">Ürünün tüm dillerdeki çevirileri.</param>
public sealed record ProductDto(
    Guid Id,
    string Sku,
    string? Barcode,
    string Name,
    string? Description,
    decimal UnitPrice,
    string Currency,
    Guid CategoryId,
    string CategoryName,
    Guid UnitOfMeasureId,
    string UnitOfMeasureCode,
    bool IsActive,
    DateTime CreatedAt,
    IReadOnlyList<ProductTranslationDto> Translations
);
