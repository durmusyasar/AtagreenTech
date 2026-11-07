using AtaGreenTech.StockService.Domain.Entities;
using AtaGreenTech.StockService.Domain.Errors;
using AtaGreenTech.StockService.Domain.Rules;
using OmniSync.Core.Application.Messaging;
using OmniSync.Core.Domain.Common;
using OmniSync.Core.Domain.Exceptions;
using OmniSync.Core.Domain.Repositories;

namespace AtaGreenTech.StockService.Application.Features.Products.Commands.UpdateProduct;

internal sealed class UpdateProductCommandHandler(
    IRepository<Product> products,
    IReadOnlyRepository<Category> categories,
    IReadOnlyRepository<UnitOfMeasure> unitsOfMeasure
) : ICommandHandler<UpdateProductCommand>
{
    public async Task Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await products.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), request.Id);

        var sku = Product.NormalizeSku(request.Sku);

        CheckRule.ThrowIf(
            new ValueMustBeUniqueRule(
                await products.ExistsAsync(p => p.Sku == sku && p.Id != request.Id, cancellationToken)
            ),
            CatalogErrors.SkuAlreadyExists,
            sku
        );
        CheckRule.ThrowIf(
            new ReferenceMustExistRule(await categories.ExistsAsync(c => c.Id == request.CategoryId, cancellationToken)),
            CatalogErrors.CategoryNotFound,
            request.CategoryId
        );
        CheckRule.ThrowIf(
            new ReferenceMustExistRule(
                await unitsOfMeasure.ExistsAsync(u => u.Id == request.UnitOfMeasureId, cancellationToken)
            ),
            CatalogErrors.UnitOfMeasureNotFound,
            request.UnitOfMeasureId
        );

        // Entity repository'den takip edilerek okunduğu için değişiklikleri command'ın transaction'ı kaydeder.
        product.Update(
            request.Name,
            sku,
            request.Description,
            request.UnitPrice,
            request.CategoryId,
            request.UnitOfMeasureId,
            request.IsActive
        );
    }
}
