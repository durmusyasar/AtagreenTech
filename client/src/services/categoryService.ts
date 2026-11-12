import type { AxiosRequestConfig } from 'axios'
import { axiosClient } from '../core/api/axiosClient'
import type { Language } from '../core/i18n/languages'
import type { PagedResult } from '../types/api'
import type { Category, CategoryQuery, CreateCategoryRequest, UpdateCategoryRequest } from '../types/category'

const BASE = '/categories'

/** Backend'deki `BaseSearchModel.MaxPageSize`. */
const MAX_PAGE_SIZE = 100

export const categoryService = {
  async list(query: CategoryQuery & { language: Language }, signal?: AbortSignal): Promise<PagedResult<Category>> {
    const { data } = await axiosClient.get<PagedResult<Category>>(BASE, { params: query, signal })
    return data
  },

  /** Üst kategori seçimi ve filtresi için tüm kategoriler (aktif ve pasif); sayfalar sırayla okunur. */
  async listAll(language: Language, signal?: AbortSignal): Promise<Category[]> {
    const categories: Category[] = []
    for (let pageNumber = 1; ; pageNumber++) {
      const page = await categoryService.list({ language, pageNumber, pageSize: MAX_PAGE_SIZE }, signal)
      categories.push(...page.items)
      if (!page.hasNextPage) return categories
    }
  },

  /** Yeni kategorinin kimliğini döndürür. */
  async create(request: CreateCategoryRequest, config?: AxiosRequestConfig): Promise<string> {
    const { data } = await axiosClient.post<string>(BASE, request, config)
    return data
  },

  async update(id: string, request: UpdateCategoryRequest, config?: AxiosRequestConfig): Promise<void> {
    await axiosClient.put(`${BASE}/${id}`, request, config)
  },
}
