import type { AxiosRequestConfig } from 'axios'
import { axiosClient } from '../core/api/axiosClient'
import type { Language } from '../core/i18n/languages'
import type {
  CreateUnitOfMeasureRequest,
  UnitOfMeasure,
  UpdateUnitOfMeasureRequest,
} from '../types/unitOfMeasure'

const BASE = '/units-of-measure'

export const unitOfMeasureService = {
  async list(params: { language: Language; isActive?: boolean }, signal?: AbortSignal): Promise<UnitOfMeasure[]> {
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
