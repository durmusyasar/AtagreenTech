using AtaGreenTech.StockService.Application.DTOs;
using AtaGreenTech.StockService.Application.Features.Categories.Commands.CreateCategory;
using AtaGreenTech.StockService.Application.Features.Categories.Commands.UpdateCategory;
using AtaGreenTech.StockService.Application.Features.Categories.Queries.GetCategoriesWithPagination;
using MediatR;
using OmniSync.Core.Domain.Common;
using OmniSync.Core.Infrastructure.Endpoints;

namespace AtaGreenTech.StockService.Api.Endpoints;

/// <summary>
/// <c>/api/v1/categories</c>: hiyerarşik ürün kategorileri.
/// </summary>
public sealed class CategoryEndpoints : IEndpointModule
{
    public const string ModuleName = "categories";

    public string Name => ModuleName;

    public void MapEndpoints(RouteGroupBuilder group)
    {
        group.MapGet("", (
            ISender sender,
            CancellationToken cancellationToken,
            int pageNumber = 1,
            int pageSize = BaseSearchModel.DefaultPageSize,
            string? searchTerm = null,
            Guid? parentCategoryId = null,
            bool? isActive = null,
            string? language = null
        ) => sender.Send(
            new GetCategoriesQuery(pageNumber, pageSize, searchTerm, parentCategoryId, isActive, language),
            cancellationToken
        ));

        group.MapPost("", async (CreateCategoryCommand command, ISender sender, CancellationToken cancellationToken) =>
        {
            var id = await sender.Send(command, cancellationToken);
            return TypedResults.Created((string?)null, id);
        });

        group.MapPut("/{id:guid}", async (
            Guid id,
            UpdateCategoryRequest request,
            ISender sender,
            CancellationToken cancellationToken
        ) =>
        {
            await sender.Send(
                new UpdateCategoryCommand(id, request.Code, request.Translations, request.ParentCategoryId, request.IsActive),
                cancellationToken
            );
            return TypedResults.NoContent();
        });
    }
}

public sealed record UpdateCategoryRequest(
    string Code,
    IReadOnlyList<CategoryTranslationDto> Translations,
    Guid? ParentCategoryId,
    bool IsActive
);
