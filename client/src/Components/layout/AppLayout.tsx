import { useCallback, useEffect, useState } from 'react'
import { Outlet } from 'react-router-dom'
import { AppNavbar } from '@/Components/AppNavbar'
import { CommandPalette } from '@/Components/command-palette/CommandPalette'
import { Footer } from '@/Components/layout/Footer'
import { Sidebar } from '@/Components/Sidebar'

const LG_QUERY = '(min-width: 1024px)'

/**
 * Oturum açık kabuk: full-width AppNavbar + below-header Sidebar + canvas content (#68).
 */
export function AppLayout() {
  const [collapsed, setCollapsed] = useState(false)
  const [mobileNavOpen, setMobileNavOpen] = useState(false)
  const [isLg, setIsLg] = useState(() =>
    typeof window !== 'undefined' ? window.matchMedia(LG_QUERY).matches : true,
  )
  const [commandOpen, setCommandOpen] = useState(false)
  const openCommandPalette = useCallback(() => setCommandOpen(true), [])

  useEffect(() => {
    const media = window.matchMedia(LG_QUERY)
    const sync = () => {
      const matches = media.matches
      setIsLg(matches)
      if (matches) setMobileNavOpen(false)
    }
    sync()
    media.addEventListener('change', sync)
    return () => media.removeEventListener('change', sync)
  }, [])

  useEffect(() => {
    if (!mobileNavOpen) return
    const onKey = (event: KeyboardEvent) => {
      if (event.key === 'Escape') setMobileNavOpen(false)
    }
    window.addEventListener('keydown', onKey)
    return () => window.removeEventListener('keydown', onKey)
  }, [mobileNavOpen])

  const toggleSidebar = useCallback(() => {
    if (isLg) setCollapsed((value) => !value)
    else setMobileNavOpen((value) => !value)
  }, [isLg])

  const closeMobileNav = useCallback(() => setMobileNavOpen(false), [])

  /** Mobilde drawer açıkken toggle ikonu “kapat”; desktop’ta collapse durumu. */
  const toggleShowsCollapsed = isLg ? collapsed : !mobileNavOpen

  return (
    <div className="relative flex min-h-screen max-w-[100vw] flex-col overflow-x-clip bg-canvas font-sans text-primary">
      <div aria-hidden className="pointer-events-none absolute inset-0 overflow-hidden">
        <div className="absolute -left-20 top-16 h-80 w-80 rounded-full bg-brand/[0.05] blur-3xl dark:bg-brand/[0.08]" />
        <div className="absolute -right-24 top-1/3 h-96 w-96 rounded-full bg-brand/[0.03] blur-3xl dark:bg-brand/[0.05]" />
        <div className="absolute bottom-0 left-1/3 h-64 w-64 rounded-full bg-elevated/40 blur-3xl dark:bg-elevated/20" />
      </div>

      <AppNavbar
        onOpenCommandPalette={openCommandPalette}
        onToggleSidebar={toggleSidebar}
        isSidebarCollapsed={toggleShowsCollapsed}
      />

      <div className="relative z-10 flex min-h-0 min-w-0 flex-1 overflow-x-clip">
        {mobileNavOpen ? (
          <button
            type="button"
            aria-label="Close navigation"
            className="fixed inset-0 top-16 z-20 bg-black/40 backdrop-blur-[2px] lg:hidden"
            onClick={closeMobileNav}
          />
        ) : null}

        <Sidebar
          collapsed={collapsed}
          mobileOpen={mobileNavOpen}
          onNavigate={closeMobileNav}
        />

        <div className="flex min-w-0 flex-1 flex-col overflow-x-clip bg-canvas">
          <main className="app-shell-main min-w-0 flex-1 px-4 py-6 sm:px-6 sm:py-8 lg:px-8">
            <div className="app-shell-rise mx-auto w-full max-w-[1180px]">
              <Outlet />
            </div>
          </main>
          <Footer />
        </div>
      </div>

      <CommandPalette open={commandOpen} onOpenChange={setCommandOpen} />
    </div>
  )
}
