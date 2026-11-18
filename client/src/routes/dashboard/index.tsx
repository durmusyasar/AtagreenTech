import { ReloadOutlined } from '@ant-design/icons'
import { useQuery } from '@tanstack/react-query'
import type { ReactNode } from 'react'
import { useAuth } from '@/core/context/useAuth'
import { useLanguage } from '@/core/context/useLanguage'
import { sessionQueries } from '@/services/sessionService'

function MetaTile({ label, children }: { label: string; children: ReactNode }) {
  return (
    <div className="rounded-2xl border border-subtle/80 bg-elevated/40 p-4 dark:border-white/10 dark:bg-elevated/30">
      <p className="text-[11px] font-semibold uppercase tracking-[0.12em] text-muted">{label}</p>
      <div className="mt-2 text-sm font-medium text-primary">{children}</div>
    </div>
  )
}

export function DashboardPage() {
  const { language, t } = useLanguage()
  const { claims } = useAuth()
  const { data, isPending, isFetching, refetch } = useQuery(sessionQueries.current())

  const expiresAt = claims?.expiresAt
    ? new Date(claims.expiresAt * 1000).toLocaleString(language)
    : '—'
  const displayName = data?.email || data?.username || claims?.email || '—'

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
        <div className="space-y-1.5">
          <p className="text-[11px] font-semibold uppercase tracking-[0.14em] text-brand">
            Atagreen
          </p>
          <h1 className="text-2xl font-semibold tracking-tight text-primary sm:text-[1.75rem]">
            {t('dashboard.title')}
          </h1>
          <p className="max-w-xl text-sm leading-relaxed text-secondary">
            {t('dashboard.subtitle', displayName)}
          </p>
        </div>

        <button
          type="button"
          onClick={() => refetch()}
          disabled={isFetching}
          className="inline-flex h-10 appearance-none items-center justify-center gap-2 self-start rounded-xl border border-subtle/80 bg-surface px-3.5 text-sm font-medium text-secondary transition-colors hover:border-brand/30 hover:text-primary disabled:opacity-60 dark:border-white/10"
        >
          <ReloadOutlined spin={isFetching} />
          {t('common.refresh')}
        </button>
      </div>

      <section className="overflow-hidden rounded-3xl border border-subtle/80 bg-surface/95 shadow-[0_18px_40px_-28px_rgb(15_23_42/0.45)] dark:border-white/10 dark:shadow-black/30">
        <div className="flex flex-wrap items-center justify-between gap-3 border-b border-subtle/70 px-5 py-4 dark:border-white/10">
          <div>
            <h2 className="text-sm font-semibold text-primary">{t('dashboard.connection')}</h2>
            <p className="mt-0.5 text-xs text-secondary">{t('dashboard.connectionHint')}</p>
          </div>

          <span
            className={[
              'inline-flex items-center gap-2 rounded-full border px-2.5 py-1 text-xs font-semibold',
              data
                ? 'border-brand/25 bg-brand/10 text-brand'
                : 'border-subtle bg-elevated text-secondary',
            ].join(' ')}
          >
            <span
              className={`h-2 w-2 rounded-full ${data ? 'bg-brand animate-pulse' : 'bg-muted'}`}
              aria-hidden
            />
            {data ? t('dashboard.connected') : isPending ? t('login.verifying') : '—'}
          </span>
        </div>

        <div className="p-5">
          {isPending ? (
            <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
              {Array.from({ length: 4 }).map((_, index) => (
                <div
                  key={index}
                  className="h-[88px] animate-pulse rounded-2xl bg-elevated/70 dark:bg-elevated/40"
                />
              ))}
            </div>
          ) : data ? (
            <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
              <MetaTile label={t('dashboard.user')}>
                <span className="break-all">{displayName}</span>
              </MetaTile>
              <MetaTile label={t('dashboard.roles')}>
                <div className="flex flex-wrap gap-1.5">
                  {data.roles.map((role) => (
                    <span
                      key={role}
                      className="rounded-lg border border-brand/20 bg-brand/10 px-2 py-0.5 text-xs font-semibold text-brand"
                    >
                      {role}
                    </span>
                  ))}
                </div>
              </MetaTile>
              <MetaTile label={t('dashboard.tenant')}>
                <span className="break-all font-mono text-xs text-secondary">
                  {data.tenantId || '—'}
                </span>
              </MetaTile>
              <MetaTile label={t('dashboard.tokenExpires')}>
                <span className="text-secondary">{expiresAt}</span>
              </MetaTile>
            </div>
          ) : null}
        </div>
      </section>
    </div>
  )
}
