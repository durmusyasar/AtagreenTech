import { useCallback, useState } from 'react'
import { Outlet } from 'react-router-dom'
import { AppNavbar } from '@/Components/AppNavbar'
import { CommandPalette } from '@/Components/command-palette/CommandPalette'
import { Footer } from '@/Components/layout/Footer'
import { Sidebar } from '@/Components/Sidebar'

/**
 * Oturum açık kabuk: AppNavbar + Sidebar + main + Footer.
 * Hepsi `bg-surface` / `border-subtle` / `bg-canvas` token’larıyla hizalıdır.
 */
export function AppLayout() {
  const [collapsed, setCollapsed] = useState(false)
  const [commandOpen, setCommandOpen] = useState(false)
  const openCommandPalette = useCallback(() => setCommandOpen(true), [])

  return (
    <div className="flex min-h-screen flex-col bg-canvas text-primary">
      <AppNavbar
        onOpenCommandPalette={openCommandPalette}
        onToggleSidebar={() => setCollapsed((value) => !value)}
        isSidebarCollapsed={collapsed}
      />

      <div className="flex min-h-0 flex-1">
        <Sidebar collapsed={collapsed} onCollapse={setCollapsed} />

        <div className="flex min-w-0 flex-1 flex-col">
          <main className="flex-1 p-4 sm:p-6">
            <Outlet />
          </main>
          <Footer />
        </div>
      </div>

      <CommandPalette open={commandOpen} onOpenChange={setCommandOpen} />
    </div>
  )
}
