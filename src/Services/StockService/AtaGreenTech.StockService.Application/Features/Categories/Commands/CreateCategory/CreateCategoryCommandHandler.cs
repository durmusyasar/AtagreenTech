using AtaGreenTech.StockService.Application.Common;
using AtaGreenTech.StockService.Domain.Entities;
using AtaGreenTech.StockService.Domain.Errors;
using AtaGreenTech.StockService.Domain.Rules;
using Microsoft.Extensions.Options;
using OmniSync.Core.Application.Messaging;
using OmniSync.Core.Domain.Common;
using OmniSync.Core.Domain.Repositories;

namespace AtaGreenTech.StockService.Application.Features.Categories.Commands.CreateCategory;

internal sealed class CreateCategoryCommandHandler(
    IRepository<Category> categories,
    IOptions<LocalizationOptions> localization
) : ICommandHandler<CreateCategoryCommand, Guid>
{
    public async Task<Guid> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var code = Category.NormalizeCode(request.Code);

        CheckRule.ThrowIf(
            new ValueMustBeUniqueRule(await categories.ExistsAsync(c => c.Code == code, cancellationToken)),
            CatalogErrors.CategoryCodeAlreadyExists,
            code
        );

        if (request.ParentCategoryId is { } parentId)
        {
            CheckRule.ThrowIf(
                new ReferenceMustExistRule(await categories.ExistsAsync(c => c.Id == parentId, cancellationToken)),
                CatalogErrors.ParentCategoryNotFound,
                parentId
            );
        }

        var category = await categories.AddAsync(
            new Category(
                code,
                request.Translations.ToEntities(),
                localization.Value.DefaultLanguage,
                request.ParentCategoryId
            ),
            cancellationToken
        );

        return category.Id;
    }
}
