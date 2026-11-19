export type DateRangeFilter = '7d' | '30d'

export interface DashboardKpiSummary {
  totalActiveProducts: number
  criticalStockCount: number
  dailyMovementCount: number
  activeUserCount: number
}

export interface StockTrendPoint {
  date: string
  inbound: number
  outbound: number
}

export interface CategoryDistributionItem {
  name: string
  value: number
}

export type RecentActivityType = 'STOCK_IN' | 'STOCK_OUT' | 'STOCK_TRANSFER' | 'USER_ACTIVE'

export interface RecentActivityItem {
  id: string
  type: RecentActivityType
  actorEmail: string
  descriptionKey: string
  descriptionParams?: unknown[]
  occurredAtUtc: string
}

export interface DashboardSummaryResponse {
  kpis: DashboardKpiSummary
  movementTrend: StockTrendPoint[]
  warehouseDistribution: CategoryDistributionItem[]
  recentActivities: RecentActivityItem[]
  stockAvailable: boolean
}
