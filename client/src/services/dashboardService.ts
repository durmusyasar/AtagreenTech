import { queryOptions } from '@tanstack/react-query'
import type { Language } from '@/core/i18n/languages'
import type {
  DashboardSummaryResponse,
  DateRangeFilter,
  RecentActivityItem,
  StockTrendPoint,
} from '@/routes/dashboard/type'
import { productService } from '@/services/productService'
import { queryKeys } from '@/services/queryKeys'
import { stockService } from '@/services/stockService'
import { userService } from '@/services/userService'

const CRITICAL_QTY = 5

function rangeStart(range: DateRangeFilter): Date {
  const days = range === '7d' ? 7 : 30
  const start = new Date()
  start.setHours(0, 0, 0, 0)
  start.setDate(start.getDate() - (days - 1))
  return start
}

function dayKey(iso: string): string {
  return iso.slice(0, 10)
}

function emptyTrend(range: DateRangeFilter): StockTrendPoint[] {
  const start = rangeStart(range)
  const days = range === '7d' ? 7 : 30
  return Array.from({ length: days }, (_, index) => {
    const d = new Date(start)
    d.setDate(start.getDate() + index)
    return { date: d.toISOString().slice(0, 10), inbound: 0, outbound: 0 }
  })
}

/**
 * Mevcut Stock/User list API'lerinden birleşik dashboard özeti üretir.
 * Stock yetkisi/aboneliği yoksa stok alanları boş + `stockAvailable: false`.
 */
export async function buildDashboardSummary(options: {
  range: DateRangeFilter
  language: Language
  includeStock: boolean
  includeUsers: boolean
  signal?: AbortSignal
}): Promise<DashboardSummaryResponse> {
  const { range, language, includeStock, includeUsers, signal } = options
  const from = rangeStart(range).toISOString()
  const todayKey = new Date().toISOString().slice(0, 10)

  const trendMap = new Map(emptyTrend(range).map((p) => [p.date, { ...p }]))
  const activities: RecentActivityItem[] = []
  let totalActiveProducts = 0
  let criticalStockCount = 0
  let dailyMovementCount = 0
  let activeUserCount = 0
  const warehouseQty = new Map<string, { name: string; value: number }>()

  const tasks: Promise<void>[] = []

  if (includeStock) {
    tasks.push(
      (async () => {
        const products = await productService.list(
          { pageNumber: 1, pageSize: 1, isActive: true, language },
          signal,
        )
        totalActiveProducts = products.totalCount
      })(),
      (async () => {
        const balances = await stockService.listBalances(
          { pageNumber: 1, pageSize: 200, includeEmpty: true, language },
          signal,
        )
        for (const row of balances.items) {
          if (row.quantity > 0 && row.quantity <= CRITICAL_QTY) criticalStockCount += 1
          const prev = warehouseQty.get(row.warehouseId)
          const next = (prev?.value ?? 0) + row.quantity
          warehouseQty.set(row.warehouseId, {
            name: row.warehouseName || row.warehouseCode,
            value: next,
          })
        }
      })(),
      (async () => {
        const movements = await stockService.listMovements(
          { pageNumber: 1, pageSize: 200, from, language },
          signal,
        )
        for (const movement of movements.items) {
          const key = dayKey(movement.createdAt)
          const point = trendMap.get(key)
          if (point) {
            if (movement.type === 'Inbound') point.inbound += movement.quantity
            if (movement.type === 'Outbound') point.outbound += movement.quantity
          }
          if (key === todayKey) dailyMovementCount += 1

          activities.push({
            id: movement.id,
            type:
              movement.type === 'Inbound'
                ? 'STOCK_IN'
                : movement.type === 'Outbound'
                  ? 'STOCK_OUT'
                  : 'STOCK_TRANSFER',
            actorEmail: movement.createdBy || '—',
            descriptionKey: 'dashboard.activity.stockMovement',
            descriptionParams: [movement.type, movement.productSku, movement.quantity],
            occurredAtUtc: movement.createdAt,
          })
        }
      })(),
    )
  }

  if (includeUsers) {
    tasks.push(
      (async () => {
        const users = await userService.list(
          { pageNumber: 1, pageSize: 50, isActive: true },
          signal,
        )
        activeUserCount = users.totalCount
        for (const user of users.items.slice(0, 8)) {
          activities.push({
            id: `user-${user.id}`,
            type: 'USER_ACTIVE',
            actorEmail: user.email,
            descriptionKey: 'dashboard.activity.activeUser',
            descriptionParams: [`${user.firstName} ${user.lastName}`.trim() || user.username],
            occurredAtUtc: user.createdAt,
          })
        }
      })(),
    )
  }

  await Promise.allSettled(tasks)

  activities.sort((a, b) => b.occurredAtUtc.localeCompare(a.occurredAtUtc))

  return {
    kpis: {
      totalActiveProducts,
      criticalStockCount,
      dailyMovementCount,
      activeUserCount,
    },
    movementTrend: [...trendMap.values()],
    warehouseDistribution: [...warehouseQty.values()]
      .filter((item) => item.value > 0)
      .sort((a, b) => b.value - a.value)
      .slice(0, 8),
    recentActivities: activities.slice(0, 12),
    stockAvailable: includeStock,
  }
}

export const dashboardQueries = {
  summary: (range: DateRangeFilter, language: Language, includeStock: boolean, includeUsers: boolean) =>
    queryOptions({
      queryKey: [...queryKeys.dashboard, 'summary', range, language, includeStock, includeUsers],
      queryFn: ({ signal }) =>
        buildDashboardSummary({ range, language, includeStock, includeUsers, signal }),
      staleTime: 30_000,
    }),
}
