import { axiosClient } from '../core/api/axiosClient'
import type { PagedResult } from '../types/api'
import type { Product, ProductQuery } from '../types/product'

const BASE = '/products'

export const productService = {
  async list(query: ProductQuery, signal?: AbortSignal): Promise<PagedResult<Product>> {
    const { data } = await axiosClient.get<PagedResult<Product>>(BASE, { params: query, signal })
    return data
  },
}
