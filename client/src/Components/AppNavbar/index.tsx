import {
  LockOutlined,
  LogoutOutlined,
  MenuFoldOutlined,
  MenuUnfoldOutlined,
  SearchOutlined,
  UserOutlined,
} from '@ant-design/icons'
import { Dropdown } from 'antd'
import { Link, useNavigate } from 'react-router-dom'
import { BrandMark } from '@/Components/BrandMark'
import { LanguageSwitcher } from '@/Components/LanguageSwitcher'
import { ThemeSwitcher } from '@/Components/ThemeSwitcher'
import { commandPaletteShortcutLabel } from '@/Components/command-palette/commandPaletteSearch'
import { useAuth } from '@/core/context/useAuth'
import { useLanguage } from '@/core/context/useLanguage'
import { useTenant } from '@/core/context/useTenant'
import type { AppNavbarProps } from '@/Components/AppNavbar/type'

/**
 * Tekil üst bar: public auth ve oturum açık kabuk aynı bileşeni kullanır.
 * `isAuthenticated` ile sol/sağ slotlar koşullu render edilir.
 */
export function AppNavbar({
  onOpenCommandPalette,
  onToggleSidebar,
  isSidebarCollapsed = false,
  className = '',
}: AppNavbarProps) {
  const { isAuthenticated, claims, signOut } = useAuth()
  const { tenant } = useTenant()
  const { t } = useLanguage()
  const navigate = useNavigate()
  const shortcut = commandPaletteShortcutLabel()

  return (
    <header
      className={`sticky top-0 z-40 h-16 w-full border-b border-subtle/80 bg-surface/90 shadow-sm shadow-slate-900/5 backdrop-blur-md dark:border-white/10 dark:shadow-none ${className}`.trim()}
    >
      <div className="mx-auto flex h-full w-full max-w-[1600px] items-center justify-between gap-3 px-4 sm:px-6">
        <div className="flex min-w-0 items-center gap-2.5 sm:gap-3">
          {isAuthenticated && onToggleSidebar ? (
            <button
              type="button"
              onClick={onToggleSidebar}
              aria-label={
                isSidebarCollapsed ? t('header.expandSidebar') : t('header.collapseSidebar')
              }
              className="inline-flex h-9 w-9 shrink-0 appearance-none items-center justify-center rounded-xl border border-subtle bg-elevated/70 text-secondary transition-colors hover:bg-elevated hover:text-primary"
            >
              {isSidebarCollapsed ? <MenuUnfoldOutlined /> : <MenuFoldOutlined />}
            </button>
          ) : null}

          <Link
            to={isAuthenticated ? '/' : '/login'}
            className="flex min-w-0 items-center gap-2.5 text-inherit"
          >
            <BrandMark size={isAuthenticated ? 28 : 36} nameStyle={{ fontSize: isAuthenticated ? 16 : 18, margin: 0 }} />
          </Link>

          {isAuthenticated && tenant ? (
            <span className="hidden items-center rounded-full border border-brand/20 bg-brand/10 px-2.5 py-0.5 text-xs font-medium lowercase text-brand sm:inline-flex">
              {tenant}
            </span>
          ) : null}
        </div>

        <div className="flex shrink-0 items-center gap-2 sm:gap-2.5">
          {isAuthenticated && onOpenCommandPalette ? (
            <button
              type="button"
              onClick={onOpenCommandPalette}
              aria-label={`${t('header.commandPalette')} (${shortcut})`}
              className="inline-flex h-9 appearance-none items-center gap-2 rounded-xl border border-subtle bg-elevated/70 px-2.5 text-xs text-secondary transition-colors hover:bg-elevated hover:text-primary sm:gap-2.5 sm:px-3"
            >
              <SearchOutlined className="text-sm" />
              <span className="hidden sm:inline">{t('header.search')}</span>
              <kbd className="hidden rounded border border-subtle bg-surface px-1.5 py-0.5 font-mono text-[11px] text-muted sm:inline">
                {shortcut}
              </kbd>
            </button>
          ) : null}

          <ThemeSwitcher />
          <LanguageSwitcher />

          {isAuthenticated ? (
            <Dropdown
              trigger={['click']}
              menu={{
                items: [
                  {
                    key: 'security',
                    icon: <LockOutlined />,
                    label: t('header.security'),
                    onClick: () => navigate('/account/security'),
                  },
                  {
                    key: 'sign-out',
                    icon: <LogoutOutlined />,
                    label: t('header.signOut'),
                    onClick: () => {
                      void signOut()
                    },
                  },
                ],
              }}
            >
              <button
                type="button"
                className="inline-flex h-9 max-w-[200px] appearance-none items-center gap-2 rounded-xl border border-subtle bg-elevated/70 px-2.5 text-xs font-medium text-secondary transition-colors hover:bg-elevated hover:text-primary"
              >
                <UserOutlined />
                <span className="hidden truncate sm:inline">
                  {claims?.email ?? claims?.subject ?? t('header.account')}
                </span>
              </button>
            </Dropdown>
          ) : null}
        </div>
      </div>
    </header>
  )
}
