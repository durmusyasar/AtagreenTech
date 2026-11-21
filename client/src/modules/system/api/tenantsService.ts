import { queryOptions } from '@tanstack/react-query'
import { axiosClient } from '@/core/api/axiosClient'
import { queryKeys } from '@/services/queryKeys'

/** Liste satırı opsiyonel modül rozeti (`isExpired` = açık ama süresi dolmuş). */
export interface TenantModuleBadge {
  code: string
  isExpired: boolean
}

/**
 * Platform tenant listesi satırı.
 * Sayım / rozetler çekmece `isEffectivelyActive` ile aynı kuralı kullanır.
 */
export interface TenantListItem {
  id: string
  name: string
  slug: string
  isActive: boolean
  /** Fiilen aktif tüm modüller (core + opsiyonel). */
  activeModuleCount: number
  /** Fiilen aktif opsiyonel (!IsCore) modüller. */
  optionalActiveModuleCount: number
  /** Açık ama süresi dolmuş opsiyonel abonelikler. */
  expiredModuleCount: number
  /** Non-core enabled rozetler. */
  activeModuleBadges: TenantModuleBadge[]
  createdAtUtc: string | null
}

export interface TenantListResponse {
  items: TenantListItem[]
  totalCount: number
  page: number
  pageSize: number
}

/**
 * Tenant modül abonelik satırı.
 * `isEnabled` ham switch; `isEffectivelyActive` liste özeti ile tutarlı.
 */
export interface TenantModuleItem {
  moduleId: string
  code: string
  name: string
  translations: Record<string, string>
  isCore: boolean
  isEnabled: boolean
  expiresAtUtc: string | null
  isEffectivelyActive: boolean
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
