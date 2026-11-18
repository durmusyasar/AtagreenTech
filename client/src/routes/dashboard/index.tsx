import { useLanguage } from '@/core/context/useLanguage'
import { useNavigationMenus } from '@/hooks/useNavigationMenus'
import { KpiSummaryGrid } from '@/routes/dashboard/components/KpiSummaryGrid'
import { RecentActivityFeed } from '@/routes/dashboard/components/RecentActivityFeed'
import { StockMovementTrendChart } from '@/routes/dashboard/components/StockMovementTrendChart'
import { WarehouseDistributionChart } from '@/routes/dashboard/components/WarehouseDistributionChart'
import { useDashboardAnalytics } from '@/routes/dashboard/hooks/useDashboardAnalytics'
import type { DateRangeFilter } from '@/routes/dashboard/type'

/** Grafik destekli genel bakış — KPI, stok trendi, depo dağılımı, son aktiviteler. */
export function DashboardPage() {
  const { t } = useLanguage()
  useNavigationMenus()
  const { data, isPending, isFetching, range, setRange, includeStock, includeUsers } =
    useDashboardAnalytics()

  const loading = isPending || isFetching

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
        <div className="space-y-1.5">
          <p className="text-[11px] font-semibold uppercase tracking-[0.14em] text-brand">Atagreen</p>
          <h1 className="text-2xl font-semibold tracking-tight text-primary sm:text-[1.75rem]">
            {t('dashboard.title')}
          </h1>
          <p className="max-w-2xl text-sm leading-relaxed text-secondary">{t('dashboard.subtitle')}</p>
        </div>

        <div className="inline-flex rounded-xl border border-subtle/80 bg-surface p-1 dark:border-white/10">
          {(['7d', '30d'] as DateRangeFilter[]).map((value) => (
            <button
              key={value}
              type="button"
              onClick={() => setRange(value)}
              className={[
                'appearance-none rounded-lg px-3 py-1.5 text-xs font-semibold transition-colors',
                range === value
                  ? 'bg-brand/15 text-brand'
                  : 'text-secondary hover:text-primary',
              ].join(' ')}
            >
              {t(`dashboard.range.${value}`)}
            </button>
          ))}
        </div>
      </div>

      {!includeStock && !includeUsers ? (
        <div className="rounded-2xl border border-subtle bg-surface px-5 py-10 text-center text-sm text-muted dark:border-white/10">
          {t('dashboard.noModules')}
        </div>
      ) : (
        <>
          <KpiSummaryGrid kpis={data?.kpis} isLoading={loading && !data} showStock={includeStock} />

          {includeStock ? (
            <div className="grid gap-4 xl:grid-cols-2">
              <StockMovementTrendChart data={data?.movementTrend ?? []} isLoading={loading && !data} />
              <WarehouseDistributionChart
                data={data?.warehouseDistribution ?? []}
                isLoading={loading && !data}
              />
            </div>
          ) : (
            <div className="rounded-2xl border border-subtle bg-surface px-5 py-8 text-sm text-secondary dark:border-white/10">
              {t('dashboard.stockInactive')}
            </div>
          )}

          <RecentActivityFeed items={data?.recentActivities ?? []} isLoading={loading && !data} />
        </>
      )}
    </div>
  )
}
