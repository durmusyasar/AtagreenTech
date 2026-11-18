import { ReloadOutlined } from '@ant-design/icons'
import { useLanguage } from '@/core/context/useLanguage'
import type { ServiceHealthResult, ServiceHealthStatus } from '@/routes/admin/system/type'
import type { ServiceHealthGridProps } from '@/routes/admin/system/components/ServiceHealthGrid/type'

function statusClass(status: ServiceHealthStatus): string {
  if (status === 'Healthy') return 'border-brand/25 bg-brand/10 text-brand'
  if (status === 'Degraded') return 'border-amber-500/30 bg-amber-500/10 text-amber-700 dark:text-amber-300'
  return 'border-rose-500/30 bg-rose-500/10 text-rose-700 dark:text-rose-300'
}

function HealthCard({ result }: { result: ServiceHealthResult }) {
  const { t } = useLanguage()
  return (
    <div className="rounded-2xl border border-subtle/80 bg-elevated/40 p-4 dark:border-white/10 dark:bg-elevated/30">
      <div className="flex items-start justify-between gap-3">
        <div>
          <p className="text-[11px] font-semibold uppercase tracking-[0.12em] text-muted">
            {result.serviceName}
          </p>
          <p className="mt-2 text-sm text-secondary">
            {result.latencyMs == null ? '—' : t('system.latencyMs', result.latencyMs)}
          </p>
        </div>
        <span
          className={`inline-flex rounded-full border px-2.5 py-1 text-xs font-semibold ${statusClass(result.status)}`}
        >
          {t(`system.status.${result.status}`)}
        </span>
      </div>
    </div>
  )
}

export function ServiceHealthGrid({
  diagnostics,
  isPending,
  isFetching,
  onRefresh,
}: ServiceHealthGridProps) {
  const { t } = useLanguage()

  return (
    <section className="overflow-hidden rounded-3xl border border-subtle/80 bg-surface/95 shadow-[0_18px_40px_-28px_rgb(15_23_42/0.45)] dark:border-white/10 dark:shadow-black/30">
      <div className="flex flex-wrap items-center justify-between gap-3 border-b border-subtle/70 px-5 py-4 dark:border-white/10">
        <div>
          <h2 className="text-sm font-semibold text-primary">{t('system.healthTitle')}</h2>
          <p className="mt-0.5 text-xs text-secondary">{t('system.healthHint')}</p>
        </div>
        <button
          type="button"
          onClick={onRefresh}
          disabled={isFetching}
          className="inline-flex h-9 appearance-none items-center justify-center gap-2 rounded-xl border border-subtle/80 bg-surface px-3 text-sm font-medium text-secondary transition-colors hover:border-brand/30 hover:text-primary disabled:opacity-60 dark:border-white/10"
        >
          <ReloadOutlined spin={isFetching} />
          {t('common.refresh')}
        </button>
      </div>

      <div className="grid gap-3 p-5 sm:grid-cols-2">
        {isPending && !diagnostics ? (
          <>
            <div className="h-24 animate-pulse rounded-2xl bg-elevated/70" />
            <div className="h-24 animate-pulse rounded-2xl bg-elevated/70" />
          </>
        ) : diagnostics ? (
          <>
            <HealthCard result={diagnostics.user} />
            <HealthCard result={diagnostics.stock} />
          </>
        ) : null}
      </div>
    </section>
  )
}
