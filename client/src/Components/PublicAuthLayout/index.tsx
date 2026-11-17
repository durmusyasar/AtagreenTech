import { Footer } from '@/Components/Footer'
import { PublicNavbar } from '@/Components/PublicNavbar'
import type { PublicAuthLayoutProps } from '@/Components/PublicAuthLayout/type'

/**
 * Public auth iskeleti: üst Navbar, ortada içerik, altta Footer.
 * Login / forgot / reset / invite / verify sayfalarında ortak kullanılır.
 */
export function PublicAuthLayout({ children, className = '' }: PublicAuthLayoutProps) {
  return (
    <div
      className={`relative flex min-h-screen flex-col overflow-hidden bg-canvas text-primary ${className}`.trim()}
    >
      <PublicNavbar />

      <div aria-hidden className="pointer-events-none absolute inset-0 flex items-center justify-center overflow-hidden">
        <div className="h-[520px] w-[520px] rounded-full bg-brand/5 blur-3xl" />
      </div>

      <main className="relative z-10 flex flex-1 items-center justify-center px-4 py-10">{children}</main>

      <Footer className="relative z-10" />
    </div>
  )
}
