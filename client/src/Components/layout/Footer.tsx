import { APP_VERSION } from '@/core/appVersion'
import { useLanguage } from '@/core/context/useLanguage'
import { useTenantTheme } from '@/core/context/useTenantTheme'

const COPYRIGHT_YEAR = new Date().getFullYear()

/** Telif / durum — shell ile aynı surface kenarlığı. */
export function Footer() {
  const { t } = useLanguage()
  const { config } = useTenantTheme()

  return (
    <footer className="w-full border-t border-subtle bg-surface px-4 py-3.5 sm:px-6">
      <div className="mx-auto flex max-w-[1200px] flex-col items-center justify-between gap-2 text-xs text-muted sm:flex-row">
        <span className="text-center text-secondary/90">
          {t('footer.line', config.name, COPYRIGHT_YEAR, APP_VERSION)}
        </span>
        <span className="inline-flex items-center gap-2 text-secondary">
          <span className="inline-flex items-center gap-1.5">
            <span className="h-1.5 w-1.5 animate-pulse rounded-full bg-brand" aria-hidden />
            {t('footer.systemStatus')}
          </span>
          <span className="text-muted">v{APP_VERSION}</span>
        </span>
      </div>
    </footer>
  )
}
