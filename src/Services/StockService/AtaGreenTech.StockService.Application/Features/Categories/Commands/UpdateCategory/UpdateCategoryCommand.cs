using AtaGreenTech.StockService.Application.DTOs;
using OmniSync.Core.Application.Messaging;

namespace AtaGreenTech.StockService.Application.Features.Categories.Commands.UpdateCategory;

/// <param name="Translations">
/// Kategorinin tüm çeviri seti: listede olmayan diller silinir. En az iki dil; sistemin varsayılan dili zorunlu.
/// </param>
public sealed record UpdateCategoryCommand(
    Guid Id,
    string Code,
    IReadOnlyList<CategoryTranslationDto> Translations,
    Guid? ParentCategoryId,
    bool IsActive
) : ICommand;
