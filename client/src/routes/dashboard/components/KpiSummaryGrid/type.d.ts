import type { DashboardKpiSummary } from '@/routes/dashboard/type'

export interface KpiSummaryGridProps {
  kpis: DashboardKpiSummary | undefined
  isLoading: boolean
  showStock: boolean
}
