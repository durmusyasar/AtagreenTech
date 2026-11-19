import { PublicAuthLayout } from '@/Components/PublicAuthLayout'
import type { AuthPageShellProps } from '@/Components/AuthPageShell/type'

/**
 * Public auth kartı: PublicAuthLayout (Navbar + Footer) içinde başlık + içerik.
 * Tema / dil seçiciler üst Navbar'dadır; IAM/tenant rozeti yoktur.
 */
export function AuthPageShell({ title, description, children }: AuthPageShellProps) {
  return (
    <PublicAuthLayout>
      <div className="auth-panel auth-panel-rise max-w-[440px] p-7 sm:p-9">
        <div className="space-y-2">
          <h1 className="text-[1.65rem] font-semibold tracking-tight text-primary">{title}</h1>
          {description ? (
            <p className="text-sm leading-relaxed text-secondary">{description}</p>
          ) : null}
        </div>
        <div className="mt-6">{children}</div>
      </div>
    </PublicAuthLayout>
  )
}
