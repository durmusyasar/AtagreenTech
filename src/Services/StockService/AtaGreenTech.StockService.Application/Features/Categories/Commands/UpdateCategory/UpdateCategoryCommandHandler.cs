using AtaGreenTech.StockService.Application.Common;
using AtaGreenTech.StockService.Application.Interfaces;
using AtaGreenTech.StockService.Domain.Entities;
using AtaGreenTech.StockService.Domain.Errors;
using AtaGreenTech.StockService.Domain.Rules;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OmniSync.Core.Application.Messaging;
using OmniSync.Core.Domain.Common;
using OmniSync.Core.Domain.Exceptions;
using OmniSync.Core.Domain.Repositories;

namespace AtaGreenTech.StockService.Application.Features.Categories.Commands.UpdateCategory;

internal sealed class UpdateCategoryCommandHandler(
    IRepository<Category> categories,
    IStockDbContext context,
    IOptions<LocalizationOptions> localization
) : ICommandHandler<UpdateCategoryCommand>
{
    public async Task Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        // Çeviriler kategoriyle birlikte yüklenir (AutoInclude); silinen diller soft delete ile işaretlenir.
        var category = await categories.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Category), request.Id);

        var code = Category.NormalizeCode(request.Code);

        CheckRule.ThrowIf(
            new ValueMustBeUniqueRule(
                await categories.ExistsAsync(c => c.Code == code && c.Id != request.Id, cancellationToken)
            ),
            CatalogErrors.CategoryCodeAlreadyExists,
            code
        );

        // Kategorinin kendisini üst kategori yapması, daha açık bir hatayla Category.Update içinde reddedilir.
        if (request.ParentCategoryId is { } parentId && parentId != request.Id)
        {
            CheckRule.ThrowIf(
                new ReferenceMustExistRule(await categories.ExistsAsync(c => c.Id == parentId, cancellationToken)),
                CatalogErrors.ParentCategoryNotFound,
                parentId
            );
            CheckRule.ThrowIf(
                new CategoryHierarchyMustNotContainCycleRule(await IsDescendantAsync(parentId, request.Id, cancellationToken)),
                CatalogErrors.CategoryHierarchyCycle,
                request.Id,
                parentId
            );
        }

        category.Update(
            code,
            request.Translations.ToEntities(),
            localization.Value.DefaultLanguage,
            request.ParentCategoryId,
            request.IsActive
        );
    }

    /// <summary>
    /// <paramref name="candidateId"/>'den köke doğru üst kategoriler izlenerek <paramref name="ancestorId"/>'ye
    /// ulaşılıp ulaşılmadığını döndürür. Mevcut veride bir döngü varsa ziyaret edilen kayıtlar tekrar okunmaz.
    /// </summary>
    private async Task<bool> IsDescendantAsync(Guid candidateId, Guid ancestorId, CancellationToken cancellationToken)
    {
        var visited = new HashSet<Guid>();
        Guid? currentId = candidateId;

        while (currentId is { } id && visited.Add(id))
        {
            if (id == ancestorId)
                return true;

            currentId = await context.Categories
                .Where(c => c.Id == id)
                .Select(c => c.ParentCategoryId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        return false;
    }
}
