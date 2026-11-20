import { queryOptions } from '@tanstack/react-query'
import { axiosClient } from '@/core/api/axiosClient'
import { queryKeys } from '@/services/queryKeys'

export interface SystemLanguage {
  id: string
  code: string
  name: string
  isDefault: boolean
  isActive: boolean
}

export interface SystemModule {
  id: string
  code: string
  serviceName: string
  iconKey: string
  displayOrder: number
  isCore: boolean
  isActive: boolean
  translations: Record<string, string>
}

export interface SystemMenuListItem {
  id: string
  code: string
  moduleId: string
  moduleCode: string
  parentId: string | null
  routePath: string
  iconKey: string
  requiredPermission: string | null
  displayOrder: number
  isActive: boolean
  translations: Record<string, string>
}

export interface SystemMenuDetail extends Omit<SystemMenuListItem, 'moduleCode'> {}

export interface SystemMenuTranslationInput {
  languageCode: string
  name: string
}

export interface UpsertSystemMenuRequest {
  code: string
  moduleId: string
  parentId?: string | null
  routePath: string
  iconKey: string
  requiredPermission?: string | null
  displayOrder: number
  isActive: boolean
  translations: SystemMenuTranslationInput[]
}

export interface PatchSystemMenuRequest {
  isActive?: boolean
  displayOrder?: number
}

export const systemMenusService = {
  async getLanguages(signal?: AbortSignal): Promise<SystemLanguage[]> {
    const { data } = await axiosClient.get<SystemLanguage[]>('/system/languages', { signal })
    return data
  },

  async getModules(signal?: AbortSignal): Promise<SystemModule[]> {
    const { data } = await axiosClient.get<SystemModule[]>('/system/modules', { signal })
    return data
  },

  async getMenus(signal?: AbortSignal): Promise<SystemMenuListItem[]> {
    const { data } = await axiosClient.get<SystemMenuListItem[]>('/system/menus', { signal })
    return data
  },

  async getMenu(id: string, signal?: AbortSignal): Promise<SystemMenuDetail> {
    const { data } = await axiosClient.get<SystemMenuDetail>(`/system/menus/${id}`, { signal })
    return data
  },

  async createMenu(body: UpsertSystemMenuRequest): Promise<{ id: string }> {
    const { data } = await axiosClient.post<{ id: string }>('/system/menus', body)
    return data
  },

  async updateMenu(id: string, body: UpsertSystemMenuRequest): Promise<void> {
    await axiosClient.put(`/system/menus/${id}`, body)
  },

  async patchMenu(id: string, body: PatchSystemMenuRequest): Promise<void> {
    await axiosClient.patch(`/system/menus/${id}`, body)
  },
}

export const systemMenusQueries = {
  languages: () =>
    queryOptions({
      queryKey: [...queryKeys.systemMenus, 'languages'],
      queryFn: ({ signal }) => systemMenusService.getLanguages(signal),
      staleTime: 60_000,
    }),
  modules: () =>
    queryOptions({
      queryKey: [...queryKeys.systemMenus, 'modules'],
      queryFn: ({ signal }) => systemMenusService.getModules(signal),
      staleTime: 60_000,
    }),
  menus: () =>
    queryOptions({
      queryKey: [...queryKeys.systemMenus, 'menus'],
      queryFn: ({ signal }) => systemMenusService.getMenus(signal),
      staleTime: 30_000,
    }),
  menu: (id: string) =>
    queryOptions({
      queryKey: [...queryKeys.systemMenus, 'menu', id],
      queryFn: ({ signal }) => systemMenusService.getMenu(id, signal),
      enabled: Boolean(id),
    }),
}
