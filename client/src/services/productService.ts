import { queryOptions } from '@tanstack/react-query'
import type { AxiosRequestConfig } from 'axios'
import { axiosClient } from '../core/api/axiosClient'
import type { Language } from '../core/i18n/languages'
import type { PagedResult } from '../types/api'
import type { CreateProductRequest, Product, ProductQuery, UpdateProductRequest } from '../types/product'
import { queryKeys } from './queryKeys'

const BASE = '/products'

export const productService = {
  async list(query: ProductQuery & { language: Language }, signal?: AbortSignal): Promise<PagedResult<Product>> {
    const { data } = await axiosClient.get<PagedResult<Product>>(BASE, { params: query, signal })
    return data
  },

  /** Yeni ürünün kimliğini döndürür. */
  async create(request: CreateProductRequest, config?: AxiosRequestConfig): Promise<string> {
    const { data } = await axiosClient.post<string>(BASE, request, config)
    return data
  },

  async update(id: string, request: UpdateProductRequest, config?: AxiosRequestConfig): Promise<void> {
    await axiosClient.put(`${BASE}/${id}`, request, config)
  },

  /** Soft delete; stok hareketi olan ürün silinemez (422 `Catalog.ProductHasStockHistory`). */
  async remove(id: string, config?: AxiosRequestConfig): Promise<void> {
    await axiosClient.delete(`${BASE}/${id}`, config)
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
