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
import { createNavigationItems, type NavItemConfig, type NavSection } from '@/config/navigation'
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

const SECTION_ORDER: NavSection[] = ['main', 'admin']

/**
 * Sol navigasyon — surface token, aktif sol şerit, bölüm grupları.
 * Marka yalnızca AppNavbar’da.
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

  const sections = useMemo(() => {
    const grouped = new Map<NavSection, NavItemConfig[]>()
    for (const section of SECTION_ORDER) grouped.set(section, [])
    for (const item of visibleNav) {
      grouped.get(item.section)?.push(item)
    }
    return SECTION_ORDER.map((section) => ({
      section,
      items: grouped.get(section) ?? [],
    })).filter((group) => group.items.length > 0)
  }, [visibleNav])

  return (
    <aside
      className={[
        'flex shrink-0 flex-col border-r border-subtle/80 bg-surface/95 backdrop-blur-sm transition-[width] duration-200 dark:border-white/10',
        collapsed ? 'w-[76px]' : 'w-[248px]',
      ].join(' ')}
    >
      <nav className="flex-1 space-y-5 overflow-y-auto px-2.5 py-4" aria-label={t('app.title')}>
        {sections.map((group) => (
          <div key={group.section} className="space-y-1">
            {!collapsed && group.section === 'admin' ? (
              <p className="px-3 pb-1 text-[10px] font-semibold uppercase tracking-[0.14em] text-muted">
                {t('menu.admin')}
              </p>
            ) : null}

            {group.items.map((item) => {
              const active = item.path === '/' ? pathname === '/' : pathname.startsWith(item.path)
              return (
                <button
                  key={item.path}
                  type="button"
                  onClick={() => navigate(item.path)}
                  title={collapsed ? t(item.label) : undefined}
                  className={[
                    'group relative flex w-full appearance-none items-center gap-3 rounded-xl px-3 py-2.5 text-left text-sm transition-all duration-150',
                    collapsed ? 'justify-center px-2' : '',
                    active
                      ? 'bg-brand/[0.12] font-semibold text-brand shadow-[inset_3px_0_0_0_rgb(var(--brand-primary))]'
                      : 'text-secondary hover:bg-elevated hover:text-primary',
                  ].join(' ')}
                >
                  <span
                    className={[
                      'inline-flex h-8 w-8 shrink-0 items-center justify-center rounded-lg text-base leading-none transition-colors',
                      active
                        ? 'bg-brand/15 text-brand'
                        : 'bg-elevated/80 text-secondary group-hover:text-primary',
                    ].join(' ')}
                  >
                    {item.icon}
                  </span>
                  {!collapsed ? <span className="truncate">{t(item.label)}</span> : null}
                </button>
              )
            })}
          </div>
        ))}
      </nav>

      <div className="border-t border-subtle/80 bg-surface p-2.5 dark:border-white/10">
        <button
          type="button"
          onClick={() => onCollapse(!collapsed)}
          aria-label={collapsed ? t('header.expandSidebar') : t('header.collapseSidebar')}
          className="flex w-full appearance-none items-center justify-center gap-2 rounded-xl px-3 py-2.5 text-xs font-medium text-secondary transition-colors hover:bg-elevated hover:text-primary"
        >
          {collapsed ? <MenuUnfoldOutlined /> : <MenuFoldOutlined />}
          {!collapsed ? <span>{t('header.collapseSidebar')}</span> : null}
        </button>
      </div>
    </aside>
  )
}
