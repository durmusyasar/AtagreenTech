import {
  AppstoreOutlined,
  BarChartOutlined,
  ColumnWidthOutlined,
  DashboardOutlined,
  LockOutlined,
  MenuFoldOutlined,
  MenuUnfoldOutlined,
  SafetyCertificateOutlined,
  ShopOutlined,
  SwapOutlined,
  TagsOutlined,
  TeamOutlined,
} from '@ant-design/icons'
import { useMemo } from 'react'
import { useLocation, useNavigate } from 'react-router-dom'
import type { SidebarProps } from '@/Components/Sidebar/type'
import { createNavigationItems } from '@/config/navigation'
import { useLanguage } from '@/core/context/useLanguage'
import { usePermissions } from '@/core/hooks/usePermissions'

const NAV_ICONS = {
  dashboard: <DashboardOutlined />,
  units: <ColumnWidthOutlined />,
  categories: <AppstoreOutlined />,
  products: <TagsOutlined />,
  warehouses: <ShopOutlined />,
  movements: <SwapOutlined />,
  balances: <BarChartOutlined />,
  security: <LockOutlined />,
  users: <TeamOutlined />,
  roles: <SafetyCertificateOutlined />,
}

/**
 * Sol navigasyon — AppNavbar ile aynı `bg-surface` / `border-subtle` token’ları.
 * Marka logosu yalnızca AppNavbar’dadır (mükerrer başlık yok).
 */
export function Sidebar({ collapsed, onCollapse }: SidebarProps) {
  const { pathname } = useLocation()
  const navigate = useNavigate()
  const { t } = useLanguage()
  const access = usePermissions()

  const visibleNav = useMemo(
    () =>
      createNavigationItems(NAV_ICONS).filter((item) =>
        access.canAccess({ roles: item.roles, permissions: item.permissions }),
      ),
    [access],
  )

  return (
    <aside
      className={[
        'flex shrink-0 flex-col border-r border-subtle bg-surface transition-[width] duration-200',
        collapsed ? 'w-[72px]' : 'w-60',
      ].join(' ')}
    >
      <nav className="flex-1 space-y-1 overflow-y-auto p-2" aria-label={t('app.title')}>
        {visibleNav.map((item) => {
          const active = item.path === '/' ? pathname === '/' : pathname.startsWith(item.path)
          return (
            <button
              key={item.path}
              type="button"
              onClick={() => navigate(item.path)}
              title={collapsed ? t(item.label) : undefined}
              className={[
                'flex w-full appearance-none items-center gap-2.5 rounded-xl border px-3 py-2.5 text-left text-sm transition-colors',
                collapsed ? 'justify-center px-2' : '',
                active
                  ? 'border-brand/20 bg-brand/12 font-semibold text-brand'
                  : 'border-transparent text-secondary hover:bg-elevated hover:text-primary',
              ].join(' ')}
            >
              <span className="text-base leading-none">{item.icon}</span>
              {!collapsed ? <span className="truncate">{t(item.label)}</span> : null}
            </button>
          )
        })}
      </nav>

      <div className="border-t border-subtle bg-surface p-2">
        <button
          type="button"
          onClick={() => onCollapse(!collapsed)}
          aria-label={collapsed ? t('header.expandSidebar') : t('header.collapseSidebar')}
          className="flex w-full appearance-none items-center justify-center gap-2 rounded-xl border border-subtle bg-elevated/60 px-3 py-2 text-xs font-medium text-secondary transition-colors hover:bg-elevated hover:text-primary"
        >
          {collapsed ? <MenuUnfoldOutlined /> : <MenuFoldOutlined />}
          {!collapsed ? (
            <span>{t('header.collapseSidebar')}</span>
          ) : null}
        </button>
      </div>
    </aside>
  )
}
