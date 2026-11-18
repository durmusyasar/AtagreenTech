import { useQuery } from '@tanstack/react-query'
import { useAuth } from '@/core/context/useAuth'
import { useLanguage } from '@/core/context/useLanguage'
import { ConnectionSessionCard } from '@/routes/admin/system/components/ConnectionSessionCard'
import { ServiceHealthGrid } from '@/routes/admin/system/components/ServiceHealthGrid'
import { useSystemDiagnostics } from '@/routes/admin/system/hooks/useSystemDiagnostics'
import { sessionQueries } from '@/services/sessionService'

/** Sistem tanı: oturum/bağlantı meta + User/Stock health. */
export function SystemPage() {
  const { language, t } = useLanguage()
  const { claims } = useAuth()
  const sessionQuery = useQuery(sessionQueries.current())
  const healthQuery = useSystemDiagnostics(true)

  const expiresAt = claims?.expiresAt
    ? new Date(claims.expiresAt * 1000).toLocaleString(language)
    : '—'
  const displayName =
    sessionQuery.data?.email || sessionQuery.data?.username || claims?.email || '—'

  return (
    <div className="space-y-6">
      <div className="space-y-1.5">
        <p className="text-[11px] font-semibold uppercase tracking-[0.14em] text-brand">
          {t('services.system')}
        </p>
        <h1 className="text-2xl font-semibold tracking-tight text-primary sm:text-[1.75rem]">
          {t('system.title')}
        </h1>
        <p className="max-w-2xl text-sm leading-relaxed text-secondary">{t('system.subtitle')}</p>
      </div>

      <ConnectionSessionCard
        session={sessionQuery.data}
        isPending={sessionQuery.isPending}
        isFetching={sessionQuery.isFetching}
        tokenExpiresLabel={expiresAt}
        displayName={displayName}
        onRefresh={() => void sessionQuery.refetch()}
      />

      <ServiceHealthGrid
        diagnostics={healthQuery.data}
        isPending={healthQuery.isPending}
        isFetching={healthQuery.isFetching}
        onRefresh={() => void healthQuery.refetch()}
      />
    </div>
  )
}
