import { AppNavbar } from '@/Components/AppNavbar'
import { Footer } from '@/Components/Footer'
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
      {/* Atmosfer: soft brand ışık + grid dokusu */}
      <div aria-hidden className="pointer-events-none absolute inset-0 overflow-hidden">
        <div className="auth-orb-a absolute -left-24 top-16 h-[420px] w-[420px] rounded-full bg-brand/[0.12] blur-3xl dark:bg-brand/[0.16]" />
        <div className="auth-orb-b absolute -right-20 bottom-10 h-[380px] w-[380px] rounded-full bg-brand/[0.08] blur-3xl dark:bg-brand/[0.12]" />
        <div
          className="absolute inset-0 opacity-[0.35] dark:opacity-[0.22]"
          style={{
            backgroundImage:
              'linear-gradient(to right, rgb(var(--border-subtle) / 0.45) 1px, transparent 1px), linear-gradient(to bottom, rgb(var(--border-subtle) / 0.45) 1px, transparent 1px)',
            backgroundSize: '48px 48px',
            maskImage: 'radial-gradient(ellipse 70% 60% at 50% 40%, black 20%, transparent 75%)',
            WebkitMaskImage:
              'radial-gradient(ellipse 70% 60% at 50% 40%, black 20%, transparent 75%)',
          }}
        />
      </div>

      <AppNavbar />

      <main className="relative z-10 flex flex-1 items-center justify-center px-4 py-10 sm:px-6 sm:py-12">
        {children}
      </main>

      <Footer className="relative z-10" />
    </div>
  )
}
