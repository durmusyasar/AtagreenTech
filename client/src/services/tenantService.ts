import { axiosClient } from '../core/api/axiosClient'
import type { TenantConfig } from '../types/tenant'
import { queryKeys } from './queryKeys'

/** Tenant marka ayarları. Oturum gerekmez; Host / X-Tenant ile çözülen tenant kullanılır. */
export const tenantService = {
  getConfig: async (signal?: AbortSignal): Promise<TenantConfig> => {
    const { data } = await axiosClient.get<TenantConfig>('/tenant/config', {
      signal,
      skipErrorNotification: true,
    })
    return data
  },
}

export const tenantConfigQuery = {
  queryKey: queryKeys.tenantConfig,
  queryFn: ({ signal }: { signal?: AbortSignal }) => tenantService.getConfig(signal),
} as const
