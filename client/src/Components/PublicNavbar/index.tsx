import { Link } from 'react-router-dom'
import { BrandMark } from '@/Components/BrandMark'
import { LanguageSwitcher } from '@/Components/LanguageSwitcher'
import { ThemeSwitcher } from '@/Components/ThemeSwitcher'
import type { PublicNavbarProps } from '@/Components/PublicNavbar/type'

/** Public auth üst çubuğu: marka + modern tema/dil kontrolleri (tenant rozeti yok). */
export function PublicNavbar({ className = '' }: PublicNavbarProps) {
  return (
    <header
      className={`sticky top-0 z-40 h-16 w-full border-b border-subtle bg-surface/85 backdrop-blur-md ${className}`.trim()}
    >
      <div className="mx-auto flex h-full max-w-7xl items-center justify-between px-4 sm:px-8">
        <Link to="/login" className="group flex min-w-0 items-center gap-2.5">
          <BrandMark size={36} nameStyle={{ fontSize: 18, margin: 0 }} />
        </Link>

        <div className="flex shrink-0 items-center gap-2.5">
          <ThemeSwitcher />
          <LanguageSwitcher />
        </div>
      </div>
    </header>
  )
}
