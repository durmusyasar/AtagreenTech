using OmniSync.Core.Application.Messaging;

namespace AtaGreenTech.StockService.Application.Features.Categories.Commands.UpdateCategory;

public sealed record UpdateCategoryCommand(
    Guid Id,
    string Name,
    string? Description,
    Guid? ParentCategoryId,
    bool IsActive
) : ICommand;
