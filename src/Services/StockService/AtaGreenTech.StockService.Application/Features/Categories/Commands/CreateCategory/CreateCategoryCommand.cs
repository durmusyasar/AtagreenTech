using OmniSync.Core.Application.Messaging;

namespace AtaGreenTech.StockService.Application.Features.Categories.Commands.CreateCategory;

public sealed record CreateCategoryCommand(string Name, string? Description, Guid? ParentCategoryId) : ICommand<Guid>;
