using AtaGreenTech.StockService.Application.Common;
using AtaGreenTech.StockService.Application.DTOs;
using AtaGreenTech.StockService.Application.Interfaces;
using Microsoft.Extensions.Options;
using OmniSync.Core.Application.Messaging;
using OmniSync.Core.Application.Pagination;

namespace AtaGreenTech.StockService.Application.Features.Categories.Queries.GetCategoriesWithPagination;

internal sealed class GetCategoriesQueryHandler(IStockDbContext context, IOptions<LocalizationOptions> localization)
    : IQueryHandler<GetCategoriesQuery, PagedResult<CategoryDto>>
{
    public async Task<PagedResult<CategoryDto>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var resolver = TranslationResolver.For(request.Language, localization);
        var defaultLanguage = resolver.DefaultLanguage;
        var categories = context.Categories;

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim();
            var lowerTerm = term.ToLowerInvariant();
            var upperTerm = term.ToUpperInvariant();

            // Kod büyük harfle saklandığı için doğrudan, adlar ise küçük harfe çevrilerek karşılaştırılır.
            categories = categories.Where(c =>
                c.Code.Contains(upperTerm) || c.Translations.Any(t => t.Name.ToLower().Contains(lowerTerm))
            );
        }

        if (request.ParentCategoryId is { } parentId)
            categories = categories.Where(c => c.ParentCategoryId == parentId);

        if (request.IsActive is { } isActive)
            categories = categories.Where(c => c.IsActive == isActive);

        var page = await categories
            .OrderBy(c => c.Translations.Where(t => t.LanguageCode == defaultLanguage).Select(t => t.Name).FirstOrDefault())
            .ThenBy(c => c.Id)
            .Select(c => new
            {
                c.Id,
                c.Code,
                c.ParentCategoryId,
                ParentCategoryCode = c.ParentCategory != null ? c.ParentCategory.Code : null,
                ParentTranslations = c.ParentCategory!.Translations
                    .Select(t => new CategoryTranslationDto(t.LanguageCode, t.Name, t.Description))
                    .ToList(),
                c.IsActive,
                c.CreatedAt,
                Translations = c.Translations
                    .OrderBy(t => t.LanguageCode)
                    .Select(t => new CategoryTranslationDto(t.LanguageCode, t.Name, t.Description))
                    .ToList(),
            })
            .ToPagedResultAsync(request.PageNumber, request.PageSize, cancellationToken);

        return page.Map(c =>
        {
            var display = resolver.Resolve(c.Translations);
            return new CategoryDto(
                c.Id,
                c.Code,
                display?.Name ?? c.Code,
                display?.Description,
                c.ParentCategoryId,
                c.ParentCategoryCode is null ? null : resolver.Resolve(c.ParentTranslations)?.Name ?? c.ParentCategoryCode,
                c.IsActive,
                c.CreatedAt,
                c.Translations
            );
        });
    }
}
