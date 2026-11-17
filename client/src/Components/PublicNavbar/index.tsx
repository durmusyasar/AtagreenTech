import { Link } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { BrandMark } from '@/Components/BrandMark'
import { LanguageSwitcher } from '@/Components/LanguageSwitcher'
import { ThemeSwitcher } from '@/Components/ThemeSwitcher'
import type { PublicNavbarProps } from '@/Components/PublicNavbar/type'

/** Public auth sayfalarının üst çubuğu: marka, tenant rozeti, tema ve dil. */
export function PublicNavbar({ tenantName }: PublicNavbarProps) {
  const { t } = useTranslation('common')

  return (
    <header className="sticky top-0 z-40 h-16 w-full border-b border-subtle bg-surface/80 backdrop-blur-md">
      <div className="mx-auto flex h-full max-w-7xl items-center justify-between px-4 sm:px-6">
        <div className="flex min-w-0 items-center gap-3">
          <Link to="/login" className="group flex min-w-0 items-center gap-2.5">
            <BrandMark size={32} nameStyle={{ fontSize: 16, margin: 0 }} />
          </Link>

          {tenantName ? (
            <span className="hidden items-center gap-1.5 rounded-full border border-subtle bg-elevated px-2.5 py-0.5 text-xs font-medium text-secondary sm:inline-flex">
              <span className="h-1.5 w-1.5 rounded-full bg-brand" aria-hidden />
              {t('header.tenant')}: <strong className="font-medium text-primary">{tenantName}</strong>
            </span>
          ) : null}
        </div>

        <div className="flex shrink-0 items-center gap-2.5">
          <ThemeSwitcher variant="toggle" />
          <LanguageSwitcher size="small" />
        </div>
      </div>
    </header>
  )
}
