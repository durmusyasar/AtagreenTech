import { PublicAuthLayout } from '@/Components/PublicAuthLayout'
import type { AuthPageShellProps } from '@/Components/AuthPageShell/type'

/**
 * Public auth kartı: PublicAuthLayout (Navbar + Footer) içinde başlık + içerik.
 * Tema / dil seçiciler üst Navbar'dadır; IAM/tenant rozeti yoktur.
 */
export function AuthPageShell({ title, description, children }: AuthPageShellProps) {
  return (
    <PublicAuthLayout>
      <div className="w-full max-w-[480px] space-y-5 rounded-2xl border border-subtle bg-surface p-7 shadow-xl shadow-black/5 sm:p-8 dark:shadow-black/30">
        <div className="space-y-1.5">
          <h1 className="text-2xl font-semibold tracking-tight text-primary">{title}</h1>
          {description ? <p className="text-sm leading-relaxed text-secondary">{description}</p> : null}
        </div>
        {children}
      </div>
    </PublicAuthLayout>
  )
}
