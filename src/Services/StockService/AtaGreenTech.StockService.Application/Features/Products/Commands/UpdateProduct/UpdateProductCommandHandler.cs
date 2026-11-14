using AtaGreenTech.StockService.Application.Common;
using AtaGreenTech.StockService.Domain.Entities;
using AtaGreenTech.StockService.Domain.Errors;
using AtaGreenTech.StockService.Domain.Pricing;
using AtaGreenTech.StockService.Domain.Rules;
using Microsoft.Extensions.Options;
using OmniSync.Core.Application.Messaging;
using OmniSync.Core.Domain.Common;
using OmniSync.Core.Domain.Exceptions;
using OmniSync.Core.Domain.Repositories;

namespace AtaGreenTech.StockService.Application.Features.Products.Commands.UpdateProduct;

internal sealed class UpdateProductCommandHandler(
    IRepository<Product> products,
    IReadOnlyRepository<Category> categories,
    IReadOnlyRepository<UnitOfMeasure> unitsOfMeasure,
    IReadOnlyRepository<StockMovement> movements,
    IOptions<LocalizationOptions> localization
) : ICommandHandler<UpdateProductCommand>
{
    public async Task Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        // Çeviriler ürünle birlikte yüklenir (AutoInclude); silinen diller soft delete ile işaretlenir.
        var product = await products.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), request.Id);

        var sku = Product.NormalizeSku(request.Sku);
        var barcode = Gtin.Normalize(request.Barcode);
        var currency = CurrencyCode.Normalize(request.Currency);

        await products.EnsureUniqueAsync(sku, barcode, product.Id, cancellationToken);
        await categories.EnsureReferencesExistAsync(
            unitsOfMeasure,
            request.CategoryId,
            request.UnitOfMeasureId,
            cancellationToken
        );

        if (currency != product.Currency)
        {
            CheckRule.ThrowIf(
                new ProductMustNotHaveStockHistoryRule(
                    await movements.ExistsAsync(m => m.ProductId == product.Id, cancellationToken)
                ),
                CatalogErrors.ProductCurrencyLocked,
                product.Currency
            );
        }

        // Entity repository'den takip edilerek okunduğu için değişiklikleri command'ın transaction'ı kaydeder.
        product.Update(
            sku,
            barcode,
            request.UnitPrice,
            currency,
            request.CategoryId,
            request.UnitOfMeasureId,
            request.Translations.ToEntities(),
            localization.Value.DefaultLanguage,
            request.IsActive
        );
    }
}
