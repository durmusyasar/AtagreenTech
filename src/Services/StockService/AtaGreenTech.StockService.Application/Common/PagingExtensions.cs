using Microsoft.EntityFrameworkCore;
using OmniSync.Core.Application.Pagination;

namespace AtaGreenTech.StockService.Application.Common;

/// <remarks>
/// OmniSync'in <c>ToPagedResultAsync</c>'i Infrastructure paketinde olduğu için Application katmanında aynı
/// davranışla tekrar tanımlanır.
/// </remarks>
internal static class PagingExtensions
{
    /// <summary>
    /// Toplam kayıt sayısını ve istenen sayfayı veritabanında okur. Sorgu benzersiz bir sırayla sıralanmış olmalıdır;
    /// aksi halde aynı kayıt iki sayfada görünebilir.
    /// </summary>
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> source,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken
    )
    {
        var totalCount = await source.CountAsync(cancellationToken);

        var skip = (long)(pageNumber - 1) * pageSize;
        List<T> items = skip >= totalCount
            ? []
            : await source.Skip((int)skip).Take(pageSize).ToListAsync(cancellationToken);

        return new PagedResult<T>(items, pageNumber, pageSize, totalCount);
    }
}
