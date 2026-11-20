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
import type { AppNavbarProps } from '@/Components/AppNavbar/type'

const chipClass =
  'inline-flex h-9 shrink-0 appearance-none items-center gap-2 rounded-xl border border-subtle bg-transparent px-2.5 text-xs font-medium text-secondary transition-colors duration-150 hover:border-brand/25 hover:bg-elevated/60 hover:text-primary'

/**
 * Tekil üst bar: public auth ve oturum açık kabuk.
 * z-40 elevation — Sidebar + content üzerinde kesintisiz üst eşik (#68).
 */
export function AppNavbar({
  onOpenCommandPalette,
  onToggleSidebar,
  isSidebarCollapsed = false,
  className = '',
}: AppNavbarProps) {
  const { isAuthenticated, claims, signOut } = useAuth()
  const { t } = useLanguage()
  const navigate = useNavigate()
  const shortcut = commandPaletteShortcutLabel()

  return (
    <header
      className={[
        'sticky top-0 z-40 h-16 w-full overflow-x-clip border-b border-subtle bg-surface',
        'shadow-[0_1px_8px_rgba(15,23,42,0.06)] dark:shadow-[0_2px_12px_rgba(0,0,0,0.35)]',
        className,
      ]
        .filter(Boolean)
        .join(' ')}
    >
      <div
        className={[
          // box-border: content-box + w-full + px-* toplam genişliği viewport dışına taşırır.
          'box-border flex h-full w-full min-w-0 items-center justify-between gap-2 px-3 sm:gap-3 sm:px-5',
          isAuthenticated ? '' : 'mx-auto max-w-7xl sm:px-8',
        ].join(' ')}
      >
        <div className="flex min-w-0 flex-1 items-center gap-2 sm:gap-2.5">
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
              size={isAuthenticated ? 28 : 36}
              nameClassName={
                isAuthenticated ? 'text-base leading-none' : 'text-lg leading-none'
              }
            />
          </Link>
        </div>

        <div className="flex min-w-0 max-w-full items-center justify-end gap-1.5 sm:gap-2">
          {isAuthenticated && onOpenCommandPalette ? (
            <button
              type="button"
              onClick={onOpenCommandPalette}
              aria-label={`${t('header.commandPalette')} (${shortcut})`}
              className={`${chipClass} max-w-[9.5rem] lg:max-w-[12rem] lg:justify-between lg:px-3`}
            >
              <span className="inline-flex min-w-0 items-center gap-2">
                <SearchOutlined className="shrink-0 text-sm opacity-80" />
                <span className="hidden truncate lg:inline">{t('header.search')}</span>
              </span>
              <kbd className="hidden shrink-0 rounded-md border border-subtle bg-elevated px-1.5 py-0.5 font-mono text-[10px] text-muted lg:inline">
                {shortcut}
              </kbd>
            </button>
          ) : null}

          <ThemeSwitcher />
          <LanguageSwitcher />

          {isAuthenticated ? (
            <Dropdown
              trigger={['click']}
              placement="bottomRight"
              // Sağ kenardaki tetikleyicide bottomLeft menüyü viewport dışına iter → yatay scroll.
              autoAdjustOverflow
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
                className={`${chipClass} min-w-0 max-w-[8.5rem] sm:max-w-[10rem] xl:max-w-[13rem]`}
              >
                <span className="inline-flex h-6 w-6 shrink-0 items-center justify-center rounded-lg bg-brand/15 text-brand">
                  <UserOutlined className="text-xs" />
                </span>
                <span className="hidden min-w-0 truncate md:inline">
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
