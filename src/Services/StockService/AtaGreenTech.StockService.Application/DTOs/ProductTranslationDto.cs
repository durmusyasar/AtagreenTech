using AtaGreenTech.StockService.Domain.Localization;

namespace AtaGreenTech.StockService.Application.DTOs;

/// <param name="LanguageCode">ISO 639-1 dil kodu, isteğe bağlı bölgeyle (ör. <c>tr</c>, <c>en</c>, <c>en-US</c>).</param>
public sealed record ProductTranslationDto(string LanguageCode, string Name, string? Description) : ITranslation;
