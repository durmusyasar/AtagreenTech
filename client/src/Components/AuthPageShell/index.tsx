import { useId } from 'react'
import { PublicAuthLayout } from '@/Components/PublicAuthLayout'
import type { AuthPageBannerTone, AuthPageShellProps } from '@/Components/AuthPageShell/type'

const MAX_WIDTH_CLASS = {
  sm: 'max-w-[440px]',
  md: 'max-w-[520px]',
} as const

const BANNER_TONE_CLASS: Record<AuthPageBannerTone, string> = {
  error: 'border-red-500/30 bg-red-500/10 text-red-600 dark:text-red-400',
  success: 'border-brand/30 bg-brand/10 text-brand',
  info: 'border-subtle/80 bg-elevated/80 text-secondary dark:border-white/10',
  warning: 'border-amber-500/30 bg-amber-500/10 text-amber-700 dark:text-amber-300',
}

/**
 * Dar public auth kartı (forgot / reset / invite / verify).
 * Geniş hero login paneli bu shell’i kullanmaz — LoginPage kendi kompozisyonunu çizer;
 * başlık tipografisi `auth-page-title` / `auth-page-description` ile paylaşılır.
 */
export function AuthPageShell({
  title,
  description,
  banner,
  footer,
  maxWidth = 'sm',
  className = '',
  panelClassName = '',
  children,
}: AuthPageShellProps) {
  const titleId = useId()

  return (
    <PublicAuthLayout className={className}>
      <section
        aria-labelledby={titleId}
        className={`auth-panel auth-panel-rise box-border w-full space-y-6 p-7 sm:p-9 ${MAX_WIDTH_CLASS[maxWidth]} ${panelClassName}`.trim()}
      >
        <header className="space-y-2">
          <h1 id={titleId} className="auth-page-title">
            {title}
          </h1>
          {description ? <div className="auth-page-description">{description}</div> : null}
        </header>

        {banner ? (
          <div role="alert" className={`auth-page-banner ${BANNER_TONE_CLASS[banner.tone]}`}>
            {banner.message}
          </div>
        ) : null}

        {children != null ? <div>{children}</div> : null}

        {footer ? <div className="text-center">{footer}</div> : null}
      </section>
    </PublicAuthLayout>
  )
}
