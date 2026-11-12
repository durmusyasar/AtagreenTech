import type { PagedResult } from '../types/api'

/** Backend'deki `BaseSearchModel.MaxPageSize`. */
export const MAX_PAGE_SIZE = 100

/** Seçim listeleri için tüm kayıtları en büyük sayfa boyutuyla sırayla okur. */
export async function fetchAllPages<T>(
  fetchPage: (page: { pageNumber: number; pageSize: number }) => Promise<PagedResult<T>>,
): Promise<T[]> {
  const items: T[] = []
  for (let pageNumber = 1; ; pageNumber++) {
    const page = await fetchPage({ pageNumber, pageSize: MAX_PAGE_SIZE })
    items.push(...page.items)
    if (!page.hasNextPage) return items
  }
}
