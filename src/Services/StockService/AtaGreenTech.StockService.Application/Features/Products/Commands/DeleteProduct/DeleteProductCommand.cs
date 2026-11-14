using OmniSync.Core.Application.Messaging;

namespace AtaGreenTech.StockService.Application.Features.Products.Commands.DeleteProduct;

/// <summary>
/// Ürünü soft delete ile siler; stok hareketi olan ürün silinemez, pasife alınmalıdır.
/// </summary>
public sealed record DeleteProductCommand(Guid Id) : ICommand;
