import { useTranslation } from 'react-i18next'
import { APP_VERSION } from '@/core/appVersion'
import type { FooterProps } from '@/Components/Footer/type'

const COPYRIGHT_YEAR = new Date().getFullYear()

/** Public auth sayfalarının alt bilgisi (telif, sistem durumu, sürüm). */
export function Footer({ className = '' }: FooterProps) {
  const { t } = useTranslation('common')

  return (
    <footer className={`w-full border-t border-subtle bg-surface px-4 py-4 sm:px-6 ${className}`.trim()}>
      <div className="mx-auto flex max-w-7xl flex-col items-center justify-between gap-2 text-xs text-muted sm:flex-row">
        <div className="flex flex-wrap items-center justify-center gap-2">
          <span className="font-medium text-secondary">Atagreen</span>
          <span aria-hidden>•</span>
          <span>
            © {COPYRIGHT_YEAR} {t('footer.rights')}
          </span>
        </div>

        <div className="flex items-center gap-4">
          <span className="inline-flex items-center gap-1.5 text-secondary">
            <span className="h-2 w-2 animate-pulse rounded-full bg-brand" aria-hidden />
            {t('footer.systemStatus')}
          </span>
          <span className="text-muted">v{APP_VERSION}</span>
        </div>
      </div>
    </footer>
  )
}
