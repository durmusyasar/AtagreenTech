import { useQuery } from '@tanstack/react-query'
import { axiosClient } from '@/core/api/axiosClient'
import type { ServiceHealthResult, SystemDiagnostics } from '@/routes/admin/system/type'
import { queryKeys } from '@/services/queryKeys'

async function probe(path: string, serviceName: string): Promise<ServiceHealthResult> {
  const started = performance.now()
  const checkedAt = new Date().toISOString()
  try {
    const response = await axiosClient.get(path, {
      timeout: 5_000,
      skipErrorNotification: true,
      validateStatus: () => true,
    })
    const latencyMs = Math.round(performance.now() - started)
    if (response.status >= 200 && response.status < 300) {
      return { serviceName, status: latencyMs > 1500 ? 'Degraded' : 'Healthy', latencyMs, checkedAt }
    }
    return { serviceName, status: 'Degraded', latencyMs, checkedAt }
  } catch {
    return {
      serviceName,
      status: 'Unreachable',
      latencyMs: null,
      checkedAt,
    }
  }
}

async function fetchDiagnostics(): Promise<SystemDiagnostics> {
  const [user, stock] = await Promise.all([
    probe('/system/health/user', 'UserService'),
    probe('/system/health/stock', 'StockService'),
  ])
  return { user, stock }
}

/** UserService + StockService readiness probe (nginx üzerinden `/health/ready`). */
export function useSystemDiagnostics(enabled: boolean) {
  return useQuery({
    queryKey: [...queryKeys.systemDiagnostics],
    queryFn: fetchDiagnostics,
    enabled,
    staleTime: 15_000,
    refetchInterval: 60_000,
  })
}
