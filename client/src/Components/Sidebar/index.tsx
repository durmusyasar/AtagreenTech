import { MenuFoldOutlined, MenuUnfoldOutlined } from '@ant-design/icons'
import { useLocation, useNavigate } from 'react-router-dom'
import type { SidebarProps } from '@/Components/Sidebar/type'
import { useLanguage } from '@/core/context/useLanguage'
import { useNavigationMenus } from '@/hooks/useNavigationMenus'
import { useNavigationStore } from '@/stores/navigationStore'
import { resolveMenuIcon } from '@/utils/iconMapper'

/**
 * Sol navigasyon — API menü grupları; yüklenirken pulse skeleton.
 * Marka yalnızca AppNavbar’da.
 */
export function Sidebar({ collapsed, onCollapse }: SidebarProps) {
  const { pathname } = useLocation()
  const navigate = useNavigate()
  const { t } = useLanguage()
  const { isPending, isError } = useNavigationMenus()
  const groups = useNavigationStore((s) => s.groups)

  return (
    <aside
      className={[
        'flex shrink-0 flex-col border-r border-subtle/80 bg-surface/95 backdrop-blur-sm transition-[width] duration-200 dark:border-white/10',
        collapsed ? 'w-[76px]' : 'w-[248px]',
      ].join(' ')}
    >
      <nav className="flex-1 space-y-5 overflow-y-auto px-2.5 py-4" aria-label={t('app.title')}>
        {isPending && groups.length === 0 ? (
          <div className="space-y-2 px-1" aria-hidden>
            {Array.from({ length: 5 }, (_, index) => (
              <div key={index} className="h-10 animate-pulse rounded-xl bg-elevated/80" />
            ))}
          </div>
        ) : null}

        {isError && groups.length === 0 && !collapsed ? (
          <p className="px-3 text-xs text-muted">{t('menu.loadError')}</p>
        ) : null}

        {groups.map((group) => (
          <div key={group.serviceName} className="space-y-1">
            {!collapsed ? (
              <p className="px-3 pb-1 text-[10px] font-semibold uppercase tracking-[0.14em] text-muted">
                {t(group.translationKey)}
              </p>
            ) : null}

            {group.items.map((item) => {
              const active = item.route === '/' ? pathname === '/' : pathname.startsWith(item.route)
              return (
                <button
                  key={item.code}
                  type="button"
                  onClick={() => navigate(item.route)}
                  title={collapsed ? t(item.translationKey) : undefined}
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
                    {resolveMenuIcon(item.icon)}
                  </span>
                  {!collapsed ? <span className="truncate">{t(item.translationKey)}</span> : null}
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
