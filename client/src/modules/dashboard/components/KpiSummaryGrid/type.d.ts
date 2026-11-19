import type { DashboardKpiSummary } from '@modules/dashboard/types'

export interface KpiSummaryGridProps {
  kpis: DashboardKpiSummary | undefined
  isLoading: boolean
  showStock: boolean
}
