import { queryOptions } from '@tanstack/react-query'
import { axiosClient } from '@/core/api/axiosClient'
import { queryKeys } from '@/services/queryKeys'

export interface TenantListItem {
  id: string
  name: string
  slug: string
  isActive: boolean
  enabledModuleCount: number
  createdAtUtc: string | null
}

export interface TenantListResponse {
  items: TenantListItem[]
  totalCount: number
  page: number
  pageSize: number
}

export interface TenantModuleItem {
  moduleId: string
  code: string
  name: string
  translations: Record<string, string>
  isCore: boolean
  isEnabled: boolean
  expiresAtUtc: string | null
}

export interface TenantModuleItemRequest {
  moduleId: string
  isEnabled: boolean
  expiresAtUtc?: string | null
}

export interface CreateTenantRequest {
  name: string
  slug: string
  modules?: TenantModuleItemRequest[]
  adminEmail?: string
  adminFirstName?: string
  adminLastName?: string
  adminPassword?: string
}

export interface CreateTenantResult {
  id: string
  name: string
  slug: string
  identitySeeded: boolean
  message?: string | null
}

export const tenantsService = {
  async list(
    params: { page?: number; pageSize?: number; search?: string },
    signal?: AbortSignal,
  ): Promise<TenantListResponse> {
    const { data } = await axiosClient.get<TenantListResponse>('/tenants', {
      params: {
        page: params.page ?? 1,
        pageSize: params.pageSize ?? 20,
        search: params.search || undefined,
      },
      signal,
    })
    return data
  },

  async create(body: CreateTenantRequest): Promise<CreateTenantResult> {
    const { data } = await axiosClient.post<CreateTenantResult>('/tenants', body)
    return data
  },

  async getModules(tenantId: string, signal?: AbortSignal): Promise<TenantModuleItem[]> {
    const { data } = await axiosClient.get<TenantModuleItem[]>(`/tenants/${tenantId}/modules`, {
      signal,
    })
    return data
  },

  async updateModules(tenantId: string, modules: TenantModuleItemRequest[]): Promise<void> {
    await axiosClient.put(`/tenants/${tenantId}/modules`, { modules })
  },
}

export const tenantsQueries = {
  list: (params: { page: number; pageSize: number; search: string }) =>
    queryOptions({
      queryKey: [...queryKeys.tenants, 'list', params] as const,
      queryFn: ({ signal }) => tenantsService.list(params, signal),
    }),

  modules: (tenantId: string) =>
    queryOptions({
      queryKey: [...queryKeys.tenants, 'modules', tenantId] as const,
      queryFn: ({ signal }) => tenantsService.getModules(tenantId, signal),
      enabled: Boolean(tenantId),
    }),
}
