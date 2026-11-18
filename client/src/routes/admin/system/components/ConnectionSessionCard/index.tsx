import { CopyOutlined, ReloadOutlined } from '@ant-design/icons'
import { useState, type ReactNode } from 'react'
import { useLanguage } from '@/core/context/useLanguage'
import type { ConnectionSessionCardProps } from '@/routes/admin/system/components/ConnectionSessionCard/type'

function MetaTile({ label, children }: { label: string; children: ReactNode }) {
  return (
    <div className="rounded-2xl border border-subtle/80 bg-elevated/40 p-4 dark:border-white/10 dark:bg-elevated/30">
      <p className="text-[11px] font-semibold uppercase tracking-[0.12em] text-muted">{label}</p>
      <div className="mt-2 text-sm font-medium text-primary">{children}</div>
    </div>
  )
}

export function ConnectionSessionCard({
  session,
  isPending,
  isFetching,
  tokenExpiresLabel,
  displayName,
  onRefresh,
}: ConnectionSessionCardProps) {
  const { t } = useLanguage()
  const [copied, setCopied] = useState(false)

  const copyTenantId = async () => {
    const value = session?.tenantId?.trim()
    if (!value || !navigator.clipboard?.writeText) return
    try {
      await navigator.clipboard.writeText(value)
      setCopied(true)
      window.setTimeout(() => setCopied(false), 1500)
    } catch {
      setCopied(false)
    }
  }

  return (
    <section className="overflow-hidden rounded-3xl border border-subtle/80 bg-surface/95 shadow-[0_18px_40px_-28px_rgb(15_23_42/0.45)] dark:border-white/10 dark:shadow-black/30">
      <div className="flex flex-wrap items-center justify-between gap-3 border-b border-subtle/70 px-5 py-4 dark:border-white/10">
        <div>
          <h2 className="text-sm font-semibold text-primary">{t('system.connection')}</h2>
          <p className="mt-0.5 text-xs text-secondary">{t('system.connectionHint')}</p>
        </div>

        <div className="flex items-center gap-2">
          <span
            className={[
              'inline-flex items-center gap-2 rounded-full border px-2.5 py-1 text-xs font-semibold',
              session
                ? 'border-brand/25 bg-brand/10 text-brand'
                : 'border-subtle bg-elevated text-secondary',
            ].join(' ')}
          >
            <span
              className={`h-2 w-2 rounded-full ${session ? 'bg-brand animate-pulse' : 'bg-muted'}`}
              aria-hidden
            />
            {session ? t('system.connected') : isPending ? t('login.verifying') : '—'}
          </span>
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
        ) : session ? (
          <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
            <MetaTile label={t('system.user')}>
              <span className="break-all">{displayName}</span>
            </MetaTile>
            <MetaTile label={t('system.roles')}>
              <div className="flex flex-wrap gap-1.5">
                {session.roles.map((role) => (
                  <span
                    key={role}
                    className="rounded-lg border border-brand/20 bg-brand/10 px-2 py-0.5 text-xs font-semibold text-brand"
                  >
                    {role}
                  </span>
                ))}
              </div>
            </MetaTile>
            <MetaTile label={t('system.tenant')}>
              <div className="flex items-start gap-2">
                <span className="min-w-0 flex-1 break-all font-mono text-xs text-secondary">
                  {session.tenantId || '—'}
                </span>
                {session.tenantId ? (
                  <button
                    type="button"
                    onClick={() => void copyTenantId()}
                    title={copied ? t('system.copied') : t('system.copyTenant')}
                    aria-label={t('system.copyTenant')}
                    className="inline-flex h-7 w-7 shrink-0 appearance-none items-center justify-center rounded-lg border border-subtle/80 text-secondary transition-colors hover:border-brand/30 hover:text-brand dark:border-white/10"
                  >
                    <CopyOutlined />
                  </button>
                ) : null}
              </div>
            </MetaTile>
            <MetaTile label={t('system.tokenExpires')}>
              <span className="text-secondary">{tokenExpiresLabel}</span>
            </MetaTile>
          </div>
        ) : null}
      </div>
    </section>
  )
}
