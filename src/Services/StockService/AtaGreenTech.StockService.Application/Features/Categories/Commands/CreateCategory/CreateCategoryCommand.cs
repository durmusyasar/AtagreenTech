using AtaGreenTech.StockService.Application.DTOs;
using OmniSync.Core.Application.Messaging;

namespace AtaGreenTech.StockService.Application.Features.Categories.Commands.CreateCategory;

/// <param name="Translations">En az iki dil; sistemin varsayılan dili (<c>Localization:DefaultLanguage</c>) zorunlu.</param>
public sealed record CreateCategoryCommand(
    string Code,
    IReadOnlyList<CategoryTranslationDto> Translations,
    Guid? ParentCategoryId = null
) : ICommand<Guid>;
