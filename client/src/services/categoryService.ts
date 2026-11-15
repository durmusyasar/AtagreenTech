import { queryOptions } from '@tanstack/react-query'
import { axiosClient } from '../core/api/axiosClient'
import type { Language } from '../core/i18n/languages'
import type { PagedResult } from '../types/api'
import type { Category, CategoryQuery } from '../types/category'
import { fetchAllPages } from './paging'
import { queryKeys } from './queryKeys'

const BASE = '/categories'

/** Liste okumaları; oluşturma / güncelleme `useAppMutation` ile yapılır. */
export const categoryService = {
  async list(query: CategoryQuery & { language: Language }, signal?: AbortSignal): Promise<PagedResult<Category>> {
    const { data } = await axiosClient.get<PagedResult<Category>>(BASE, { params: query, signal })
    return data
  },

  /** Üst kategori seçimi ve filtresi için tüm kategoriler (aktif ve pasif); sayfalar sırayla okunur. */
  listAll(language: Language, signal?: AbortSignal): Promise<Category[]> {
    return fetchAllPages((page) => categoryService.list({ language, ...page }, signal))
  },
}

/** Kategori adları istenen dile göre çözümlendiğinden dil anahtarın parçasıdır. */
export const categoryQueries = {
  list: (query: CategoryQuery & { language: Language }) =>
    queryOptions({
      queryKey: [...queryKeys.categories, 'list', query],
      queryFn: ({ signal }) => categoryService.list(query, signal),
    }),

  listAll: (language: Language) =>
    queryOptions({
      queryKey: [...queryKeys.categories, 'all', language],
      queryFn: ({ signal }) => categoryService.listAll(language, signal),
    }),
}
