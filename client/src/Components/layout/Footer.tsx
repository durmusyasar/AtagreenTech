import { APP_VERSION } from '@/core/appVersion'
import { useLanguage } from '@/core/context/useLanguage'
import { useTenantTheme } from '@/core/context/useTenantTheme'

/** Modül yükleme yılı; render sırasında `Date` çağrılmasın diye sabitlenir. */
const COPYRIGHT_YEAR = new Date().getFullYear()

/** Telif ve sürüm — AppNavbar / Sidebar ile aynı surface token’ı. */
export function Footer() {
  const { t } = useLanguage()
  const { config } = useTenantTheme()

  return (
    <footer className="w-full border-t border-subtle bg-surface px-4 py-4 sm:px-6">
      <div className="mx-auto flex max-w-[1600px] flex-col items-center justify-between gap-2 text-xs text-muted sm:flex-row">
        <span className="text-center text-secondary">
          {t('footer.line', config.name, COPYRIGHT_YEAR, APP_VERSION)}
        </span>
        <span className="inline-flex items-center gap-1.5 text-secondary">
          <span className="h-2 w-2 animate-pulse rounded-full bg-brand" aria-hidden />
          {t('footer.systemStatus')}
          <span className="text-muted">v{APP_VERSION}</span>
        </span>
      </div>
    </footer>
  )
}
