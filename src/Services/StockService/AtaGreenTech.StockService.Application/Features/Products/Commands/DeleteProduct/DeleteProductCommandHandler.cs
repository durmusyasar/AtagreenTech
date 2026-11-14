using AtaGreenTech.StockService.Domain.Entities;
using AtaGreenTech.StockService.Domain.Errors;
using AtaGreenTech.StockService.Domain.Rules;
using OmniSync.Core.Application.Messaging;
using OmniSync.Core.Domain.Common;
using OmniSync.Core.Domain.Exceptions;
using OmniSync.Core.Domain.Repositories;

namespace AtaGreenTech.StockService.Application.Features.Products.Commands.DeleteProduct;

internal sealed class DeleteProductCommandHandler(
    IRepository<Product> products,
    IReadOnlyRepository<StockMovement> movements
) : ICommandHandler<DeleteProductCommand>
{
    public async Task Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await products.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), request.Id);

        // Bakiyeler yalnızca hareketlerle oluştuğundan hareketi olmayan ürünün bakiyesi de yoktur.
        CheckRule.ThrowIf(
            new ProductMustNotHaveStockHistoryRule(
                await movements.ExistsAsync(m => m.ProductId == product.Id, cancellationToken)
            ),
            CatalogErrors.ProductHasStockHistory,
            product.Sku
        );

        // Çeviriler cascade ile birlikte silinir; OmniSync ikisini de soft delete'e çevirir.
        products.Delete(product);
    }
}
