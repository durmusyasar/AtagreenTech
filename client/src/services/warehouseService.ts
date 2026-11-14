import { queryOptions } from '@tanstack/react-query'
import type { AxiosRequestConfig } from 'axios'
import { axiosClient } from '../core/api/axiosClient'
import type { PagedResult } from '../types/api'
import type { CreateWarehouseRequest, UpdateWarehouseRequest, Warehouse, WarehouseQuery } from '../types/warehouse'
import { fetchAllPages } from './paging'
import { queryKeys } from './queryKeys'

const BASE = '/warehouses'

export const warehouseService = {
  async list(query: WarehouseQuery, signal?: AbortSignal): Promise<PagedResult<Warehouse>> {
    const { data } = await axiosClient.get<PagedResult<Warehouse>>(BASE, { params: query, signal })
    return data
  },

  /** Depo seçimleri ve filtreleri için tüm depolar, koda göre sıralı. */
  listAll(query: Pick<WarehouseQuery, 'isActive'> = {}, signal?: AbortSignal): Promise<Warehouse[]> {
    return fetchAllPages((page) => warehouseService.list({ ...query, ...page }, signal))
  },

  /** Yeni deponun kimliğini döndürür. */
  async create(request: CreateWarehouseRequest, config?: AxiosRequestConfig): Promise<string> {
    const { data } = await axiosClient.post<string>(BASE, request, config)
    return data
  },

  /** Durum değişikliği de bu istekle yapılır; ayrı bir durum uç noktası yoktur. */
  async update(id: string, request: UpdateWarehouseRequest, config?: AxiosRequestConfig): Promise<void> {
    await axiosClient.put(`${BASE}/${id}`, request, config)
  },
}

export const warehouseQueries = {
  list: (query: WarehouseQuery) =>
    queryOptions({
      queryKey: [...queryKeys.warehouses, 'list', query],
      queryFn: ({ signal }) => warehouseService.list(query, signal),
    }),

  listAll: (query: Pick<WarehouseQuery, 'isActive'> = {}) =>
    queryOptions({
      queryKey: [...queryKeys.warehouses, 'all', query],
      queryFn: ({ signal }) => warehouseService.listAll(query, signal),
    }),
}
