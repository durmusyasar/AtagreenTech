import { useCallback, useState } from 'react'
import { Outlet } from 'react-router-dom'
import { AppNavbar } from '@/Components/AppNavbar'
import { CommandPalette } from '@/Components/command-palette/CommandPalette'
import { Footer } from '@/Components/layout/Footer'
import { Sidebar } from '@/Components/Sidebar'

/**
 * Oturum açık kabuk: AppNavbar + Sidebar + main + Footer.
 * Canvas üzerinde soft brand atmosfer; içerik panelleri surface token’ı kullanır.
 */
export function AppLayout() {
  const [collapsed, setCollapsed] = useState(false)
  const [commandOpen, setCommandOpen] = useState(false)
  const openCommandPalette = useCallback(() => setCommandOpen(true), [])

  return (
    <div className="relative flex min-h-screen max-w-[100vw] flex-col overflow-x-clip bg-canvas text-primary">
      <div aria-hidden className="pointer-events-none absolute inset-0 overflow-hidden">
        <div className="absolute -left-24 top-24 h-72 w-72 rounded-full bg-brand/[0.07] blur-3xl dark:bg-brand/[0.12]" />
        <div className="absolute -right-16 bottom-10 h-80 w-80 rounded-full bg-brand/[0.05] blur-3xl dark:bg-brand/[0.08]" />
      </div>

      <AppNavbar
        onOpenCommandPalette={openCommandPalette}
        onToggleSidebar={() => setCollapsed((value) => !value)}
        isSidebarCollapsed={collapsed}
      />

      <div className="relative z-10 flex min-h-0 min-w-0 flex-1 overflow-x-clip">
        <Sidebar collapsed={collapsed} onCollapse={setCollapsed} />

        <div className="flex min-w-0 flex-1 flex-col overflow-x-clip">
          <main className="app-shell-main min-w-0 flex-1 px-4 py-5 sm:px-6 sm:py-7 lg:px-8">
            <div className="mx-auto w-full max-w-[1200px] app-shell-rise">
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
