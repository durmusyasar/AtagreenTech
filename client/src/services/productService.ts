import { queryOptions } from '@tanstack/react-query'
import { axiosClient } from '../core/api/axiosClient'
import type { Language } from '../core/i18n/languages'
import type { PagedResult } from '../types/api'
import type { Product, ProductQuery } from '../types/product'
import { queryKeys } from './queryKeys'

const BASE = '/products'

/** Liste okumaları; oluşturma / güncelleme / silme `useAppMutation` ile yapılır. */
export const productService = {
  async list(query: ProductQuery & { language: Language }, signal?: AbortSignal): Promise<PagedResult<Product>> {
    const { data } = await axiosClient.get<PagedResult<Product>>(BASE, { params: query, signal })
    return data
  },
}

/** Ürün ve kategori adları istenen dile göre çözümlendiğinden dil anahtarın parçasıdır. */
export const productQueries = {
  list: (query: ProductQuery & { language: Language }) =>
    queryOptions({
      queryKey: [...queryKeys.products, 'list', query],
      queryFn: ({ signal }) => productService.list(query, signal),
    }),
}
