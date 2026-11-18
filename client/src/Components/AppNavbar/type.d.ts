export interface AppNavbarProps {
  /** Oturum açıkken ⌘K / Ctrl+K komut paletini açar. */
  onOpenCommandPalette?: () => void
  /** Oturum açıkken sol menüyü daraltır / genişletir. */
  onToggleSidebar?: () => void
  isSidebarCollapsed?: boolean
  className?: string
}
