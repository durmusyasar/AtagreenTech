using AtaGreenTech.StockService.Application.DTOs;
using OmniSync.Core.Application.Messaging;
using OmniSync.Core.Application.Pagination;
using OmniSync.Core.Domain.Common;

namespace AtaGreenTech.StockService.Application.Features.Categories.Queries.GetCategoriesWithPagination;

/// <param name="SearchTerm">Kategori adında (büyük/küçük harf duyarsız) aranan metin.</param>
/// <param name="ParentCategoryId">Verilirse yalnızca bu kategorinin doğrudan alt kategorileri listelenir.</param>
public sealed record GetCategoriesQuery(
    int PageNumber = 1,
    int PageSize = BaseSearchModel.DefaultPageSize,
    string? SearchTerm = null,
    Guid? ParentCategoryId = null,
    bool? IsActive = null
) : IQuery<PagedResult<CategoryDto>>;
