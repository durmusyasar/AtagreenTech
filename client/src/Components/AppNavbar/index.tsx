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

const chipClass =
  'inline-flex h-9 appearance-none items-center gap-2 rounded-xl border border-subtle/80 bg-elevated/50 px-2.5 text-xs font-medium text-secondary transition-all duration-150 hover:border-brand/25 hover:bg-elevated hover:text-primary dark:border-white/10'

/**
 * Tekil üst bar: public auth ve oturum açık kabuk.
 * Authenticated modda tam genişlik; public’te marka odaklı dar kompozisyon.
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
      className={[
        'sticky top-0 z-40 h-16 w-full border-b border-subtle/70 bg-surface/85 backdrop-blur-xl',
        'shadow-[0_1px_0_rgb(15_23_42/0.03)] dark:border-white/10 dark:shadow-none',
        className,
      ]
        .filter(Boolean)
        .join(' ')}
    >
      <div
        className={[
          'flex h-full w-full items-center justify-between gap-3 px-4 sm:px-6',
          isAuthenticated ? '' : 'mx-auto max-w-7xl sm:px-8',
        ].join(' ')}
      >
        <div className="flex min-w-0 items-center gap-2.5 sm:gap-3">
          {isAuthenticated && onToggleSidebar ? (
            <button
              type="button"
              onClick={onToggleSidebar}
              aria-label={
                isSidebarCollapsed ? t('header.expandSidebar') : t('header.collapseSidebar')
              }
              className={`${chipClass} w-9 justify-center px-0`}
            >
              {isSidebarCollapsed ? <MenuUnfoldOutlined /> : <MenuFoldOutlined />}
            </button>
          ) : null}

          <Link
            to={isAuthenticated ? '/' : '/login'}
            className="flex min-w-0 items-center gap-2.5 text-inherit transition-opacity hover:opacity-90"
          >
            <BrandMark
              size={isAuthenticated ? 30 : 36}
              nameStyle={{ fontSize: isAuthenticated ? 17 : 18, margin: 0 }}
            />
          </Link>

          {isAuthenticated && tenant ? (
            <span className="hidden items-center gap-1.5 rounded-full border border-brand/20 bg-brand/[0.12] px-2.5 py-1 text-[11px] font-semibold lowercase tracking-wide text-brand sm:inline-flex">
              <span className="h-1.5 w-1.5 rounded-full bg-brand" aria-hidden />
              {tenant}
            </span>
          ) : null}
        </div>

        <div className="flex shrink-0 items-center gap-2">
          {isAuthenticated && onOpenCommandPalette ? (
            <button
              type="button"
              onClick={onOpenCommandPalette}
              aria-label={`${t('header.commandPalette')} (${shortcut})`}
              className={`${chipClass} sm:min-w-[168px] sm:justify-between sm:px-3`}
            >
              <span className="inline-flex items-center gap-2">
                <SearchOutlined className="text-sm opacity-80" />
                <span className="hidden sm:inline">{t('header.search')}</span>
              </span>
              <kbd className="hidden rounded-md border border-subtle/80 bg-surface/90 px-1.5 py-0.5 font-mono text-[10px] text-muted sm:inline">
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
              <button type="button" className={`${chipClass} max-w-[220px]`}>
                <span className="inline-flex h-6 w-6 items-center justify-center rounded-lg bg-brand/15 text-brand">
                  <UserOutlined className="text-xs" />
                </span>
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
