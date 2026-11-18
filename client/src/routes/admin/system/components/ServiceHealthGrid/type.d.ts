import type { SystemDiagnostics } from '@/routes/admin/system/type'

export interface ServiceHealthGridProps {
  diagnostics: SystemDiagnostics | undefined
  isPending: boolean
  isFetching: boolean
  onRefresh: () => void
}
