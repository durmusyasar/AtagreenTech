export type ServiceHealthStatus = 'Healthy' | 'Degraded' | 'Unreachable'

export interface ServiceHealthResult {
  serviceName: string
  status: ServiceHealthStatus
  latencyMs: number | null
  checkedAt: string
}

export interface SystemDiagnostics {
  user: ServiceHealthResult
  stock: ServiceHealthResult
}
