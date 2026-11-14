import { queryOptions } from '@tanstack/react-query'
import type { AxiosRequestConfig } from 'axios'
import { axiosClient } from '../core/api/axiosClient'
import type { Language } from '../core/i18n/languages'
import type {
  CreateUnitOfMeasureRequest,
  UnitOfMeasure,
  UpdateUnitOfMeasureRequest,
} from '../types/unitOfMeasure'
import { queryKeys } from './queryKeys'

const BASE = '/units-of-measure'

type UnitOfMeasureQuery = { language: Language; isActive?: boolean }

export const unitOfMeasureService = {
  async list(params: UnitOfMeasureQuery, signal?: AbortSignal): Promise<UnitOfMeasure[]> {
    const { data } = await axiosClient.get<UnitOfMeasure[]>(BASE, { params, signal })
    return data
  },

  /** Yeni birimin kimliğini döndürür. */
  async create(request: CreateUnitOfMeasureRequest, config?: AxiosRequestConfig): Promise<string> {
    const { data } = await axiosClient.post<string>(BASE, request, config)
    return data
  },

  async update(id: string, request: UpdateUnitOfMeasureRequest, config?: AxiosRequestConfig): Promise<void> {
    await axiosClient.put(`${BASE}/${id}`, request, config)
  },
}

/** Birim adları istenen dile göre çözümlendiğinden dil anahtarın parçasıdır. */
export const unitOfMeasureQueries = {
  list: (query: UnitOfMeasureQuery) =>
    queryOptions({
      queryKey: [...queryKeys.unitsOfMeasure, 'list', query],
      queryFn: ({ signal }) => unitOfMeasureService.list(query, signal),
    }),
}
