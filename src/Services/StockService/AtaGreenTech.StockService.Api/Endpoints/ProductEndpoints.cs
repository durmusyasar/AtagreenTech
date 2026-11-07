using AtaGreenTech.StockService.Application.Features.Products.Commands.CreateProduct;
using AtaGreenTech.StockService.Application.Features.Products.Commands.UpdateProduct;
using AtaGreenTech.StockService.Application.Features.Products.Queries.GetProductById;
using AtaGreenTech.StockService.Application.Features.Products.Queries.GetProductsWithPagination;
using MediatR;
using OmniSync.Core.Domain.Common;
using OmniSync.Core.Infrastructure.Endpoints;

namespace AtaGreenTech.StockService.Api.Endpoints;

/// <summary>
/// <c>/api/v1/products</c>: ürün kataloğu.
/// </summary>
public sealed class ProductEndpoints : IEndpointModule
{
    public const string ModuleName = "products";

    public string Name => ModuleName;

    public void MapEndpoints(RouteGroupBuilder group)
    {
        group.MapGet("", (
            ISender sender,
            CancellationToken cancellationToken,
            int pageNumber = 1,
            int pageSize = BaseSearchModel.DefaultPageSize,
            string? searchTerm = null,
            Guid? categoryId = null,
            bool? isActive = null
        ) => sender.Send(new GetProductsQuery(pageNumber, pageSize, searchTerm, categoryId, isActive), cancellationToken));

        group.MapGet("/{id:guid}", (Guid id, ISender sender, CancellationToken cancellationToken) =>
            sender.Send(new GetProductByIdQuery(id), cancellationToken));

        group.MapPost("", async (CreateProductCommand command, ISender sender, CancellationToken cancellationToken) =>
        {
            var id = await sender.Send(command, cancellationToken);
            return TypedResults.Created($"{EndpointRoutes.For(ModuleName)}/{id}", id);
        });

        group.MapPut("/{id:guid}", async (
            Guid id,
            UpdateProductRequest request,
            ISender sender,
            CancellationToken cancellationToken
        ) =>
        {
            await sender.Send(
                new UpdateProductCommand(
                    id,
                    request.Name,
                    request.Sku,
                    request.Description,
                    request.UnitPrice,
                    request.CategoryId,
                    request.UnitOfMeasureId,
                    request.IsActive
                ),
                cancellationToken
            );
            return TypedResults.NoContent();
        });
    }
}

public sealed record UpdateProductRequest(
    string Name,
    string Sku,
    string? Description,
    decimal UnitPrice,
    Guid CategoryId,
    Guid UnitOfMeasureId,
    bool IsActive
);
