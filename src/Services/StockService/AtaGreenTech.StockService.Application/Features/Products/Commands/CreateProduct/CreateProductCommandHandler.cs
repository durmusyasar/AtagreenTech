using AtaGreenTech.StockService.Domain.Entities;
using AtaGreenTech.StockService.Domain.Errors;
using AtaGreenTech.StockService.Domain.Rules;
using OmniSync.Core.Application.Messaging;
using OmniSync.Core.Domain.Common;
using OmniSync.Core.Domain.Repositories;

namespace AtaGreenTech.StockService.Application.Features.Products.Commands.CreateProduct;

internal sealed class CreateProductCommandHandler(
    IRepository<Product> products,
    IReadOnlyRepository<Category> categories,
    IReadOnlyRepository<UnitOfMeasure> unitsOfMeasure
) : ICommandHandler<CreateProductCommand, Guid>
{
    public async Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var sku = Product.NormalizeSku(request.Sku);

        CheckRule.ThrowIf(
            new ValueMustBeUniqueRule(await products.ExistsAsync(p => p.Sku == sku, cancellationToken)),
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

        var product = await products.AddAsync(
            new Product(request.Name, sku, request.Description, request.UnitPrice, request.CategoryId, request.UnitOfMeasureId),
            cancellationToken
        );

        return product.Id;
    }
}
