using AtaGreenTech.StockService.Application.Common;
using AtaGreenTech.StockService.Domain.Entities;
using AtaGreenTech.StockService.Domain.Pricing;
using Microsoft.Extensions.Options;
using OmniSync.Core.Application.Messaging;
using OmniSync.Core.Domain.Repositories;

namespace AtaGreenTech.StockService.Application.Features.Products.Commands.CreateProduct;

internal sealed class CreateProductCommandHandler(
    IRepository<Product> products,
    IReadOnlyRepository<Category> categories,
    IReadOnlyRepository<UnitOfMeasure> unitsOfMeasure,
    IOptions<LocalizationOptions> localization
) : ICommandHandler<CreateProductCommand, Guid>
{
    public async Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var sku = Product.NormalizeSku(request.Sku);
        var barcode = Gtin.Normalize(request.Barcode);

        await products.EnsureUniqueAsync(sku, barcode, productId: null, cancellationToken);
        await categories.EnsureReferencesExistAsync(
            unitsOfMeasure,
            request.CategoryId,
            request.UnitOfMeasureId,
            cancellationToken
        );

        var product = await products.AddAsync(
            new Product(
                sku,
                barcode,
                request.UnitPrice,
                request.Currency,
                request.CategoryId,
                request.UnitOfMeasureId,
                request.Translations.ToEntities(),
                localization.Value.DefaultLanguage
            ),
            cancellationToken
        );

        return product.Id;
    }
}
