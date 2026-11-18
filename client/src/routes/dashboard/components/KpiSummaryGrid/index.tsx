import { useLanguage } from '@/core/context/useLanguage'
import type { KpiSummaryGridProps } from '@/routes/dashboard/components/KpiSummaryGrid/type'

function KpiCard({ label, value, isLoading }: { label: string; value: string; isLoading: boolean }) {
  return (
    <div className="rounded-2xl border border-subtle/80 bg-surface/95 p-4 dark:border-white/10">
      <p className="text-[11px] font-semibold uppercase tracking-[0.12em] text-muted">{label}</p>
      {isLoading ? (
        <div className="mt-3 h-8 w-20 animate-pulse rounded-lg bg-elevated/80" />
      ) : (
        <p className="mt-2 text-2xl font-semibold tracking-tight text-primary">{value}</p>
      )}
    </div>
  )
}

export function KpiSummaryGrid({ kpis, isLoading, showStock }: KpiSummaryGridProps) {
  const { t } = useLanguage()
  const format = (n: number | undefined) => (n == null ? '—' : n.toLocaleString())

  return (
    <div className="grid gap-3 sm:grid-cols-2 xl:grid-cols-4">
      {showStock ? (
        <>
          <KpiCard
            label={t('dashboard.kpi.activeProducts')}
            value={format(kpis?.totalActiveProducts)}
            isLoading={isLoading}
          />
          <KpiCard
            label={t('dashboard.kpi.criticalStock')}
            value={format(kpis?.criticalStockCount)}
            isLoading={isLoading}
          />
          <KpiCard
            label={t('dashboard.kpi.dailyMovements')}
            value={format(kpis?.dailyMovementCount)}
            isLoading={isLoading}
          />
        </>
      ) : null}
      <KpiCard
        label={t('dashboard.kpi.activeUsers')}
        value={format(kpis?.activeUserCount)}
        isLoading={isLoading}
      />
    </div>
  )
}
