import { queryOptions } from '@tanstack/react-query'
import { axiosClient } from '../core/api/axiosClient'
import type { PagedResult } from '../types/api'
import type { Product, ProductQuery } from '../types/product'
import { queryKeys } from './queryKeys'

const BASE = '/products'

export const productService = {
  async list(query: ProductQuery, signal?: AbortSignal): Promise<PagedResult<Product>> {
    const { data } = await axiosClient.get<PagedResult<Product>>(BASE, { params: query, signal })
    return data
  },
}

export const productQueries = {
  list: (query: ProductQuery) =>
    queryOptions({
      queryKey: [...queryKeys.products, 'list', query],
      queryFn: ({ signal }) => productService.list(query, signal),
    }),
}
