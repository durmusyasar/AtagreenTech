using AtaGreenTech.StockService.Application.Common;
using AtaGreenTech.StockService.Application.DTOs;
using AtaGreenTech.StockService.Application.Interfaces;
using OmniSync.Core.Application.Messaging;
using OmniSync.Core.Application.Pagination;

namespace AtaGreenTech.StockService.Application.Features.Categories.Queries.GetCategoriesWithPagination;

internal sealed class GetCategoriesQueryHandler(IStockDbContext context)
    : IQueryHandler<GetCategoriesQuery, PagedResult<CategoryDto>>
{
    public Task<PagedResult<CategoryDto>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = context.Categories;

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim().ToLowerInvariant();
            categories = categories.Where(c => c.Name.ToLower().Contains(term));
        }

        if (request.ParentCategoryId is { } parentId)
            categories = categories.Where(c => c.ParentCategoryId == parentId);

        if (request.IsActive is { } isActive)
            categories = categories.Where(c => c.IsActive == isActive);

        return categories
            .OrderBy(c => c.Name)
            .ThenBy(c => c.Id)
            .Select(c => new CategoryDto(
                c.Id,
                c.Name,
                c.Description,
                c.ParentCategoryId,
                c.ParentCategory != null ? c.ParentCategory.Name : null,
                c.IsActive,
                c.CreatedAt
            ))
            .ToPagedResultAsync(request.PageNumber, request.PageSize, cancellationToken);
    }
}
