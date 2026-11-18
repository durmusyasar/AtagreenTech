import { useLocation, useNavigate } from 'react-router-dom'
import type { SidebarProps } from '@/Components/Sidebar/type'
import { useLanguage } from '@/core/context/useLanguage'
import { useNavigationMenus } from '@/hooks/useNavigationMenus'
import { useNavigationStore } from '@/stores/navigationStore'
import { resolveMenuIcon } from '@/utils/iconMapper'

/**
 * Sol navigasyon — düz, kartsız satırlar; API menü grupları.
 * Marka yalnızca AppNavbar’da. Collapse kontrolü navbar’da.
 */
export function Sidebar({ collapsed }: SidebarProps) {
  const { pathname } = useLocation()
  const navigate = useNavigate()
  const { t } = useLanguage()
  const { isPending, isError } = useNavigationMenus()
  const groups = useNavigationStore((s) => s.groups)

  return (
    <aside
      className={[
        'flex shrink-0 flex-col border-r border-subtle/60 bg-surface/80 backdrop-blur-md transition-[width] duration-200 dark:border-white/[0.06]',
        collapsed ? 'w-[68px]' : 'w-[232px]',
      ].join(' ')}
    >
      <nav
        className="flex-1 space-y-4 overflow-y-auto overflow-x-hidden px-2 py-3"
        aria-label={t('app.title')}
      >
        {isPending && groups.length === 0 ? (
          <div className="space-y-1.5 px-1" aria-hidden>
            {Array.from({ length: 5 }, (_, index) => (
              <div key={index} className="h-9 animate-pulse rounded-lg bg-elevated/50" />
            ))}
          </div>
        ) : null}

        {isError && groups.length === 0 && !collapsed ? (
          <p className="px-2.5 text-xs text-muted">{t('menu.loadError')}</p>
        ) : null}

        {groups.map((group) => (
          <div key={group.serviceName} className="space-y-0.5">
            {!collapsed ? (
              <p className="px-2.5 pb-1 pt-0.5 text-[10px] font-medium uppercase tracking-[0.16em] text-muted/80">
                {t(group.translationKey)}
              </p>
            ) : (
              <div className="mx-auto mb-1 h-px w-6 bg-subtle/60 dark:bg-white/10" aria-hidden />
            )}

            {group.items.map((item) => {
              const active = item.route === '/' ? pathname === '/' : pathname.startsWith(item.route)
              return (
                <button
                  key={item.code}
                  type="button"
                  onClick={() => navigate(item.route)}
                  title={collapsed ? t(item.translationKey) : undefined}
                  className={[
                    'group relative flex w-full appearance-none items-center gap-2.5 rounded-lg border border-transparent bg-transparent px-2.5 py-2 text-left text-[13px] transition-colors duration-150',
                    collapsed ? 'justify-center px-2' : '',
                    active
                      ? 'bg-brand/[0.1] font-medium text-brand'
                      : 'text-secondary hover:bg-elevated/50 hover:text-primary',
                  ].join(' ')}
                >
                  {active ? (
                    <span
                      className="absolute inset-y-1.5 left-0 w-[2px] rounded-full bg-brand"
                      aria-hidden
                    />
                  ) : null}
                  <span
                    className={[
                      'inline-flex h-7 w-7 shrink-0 items-center justify-center rounded-md text-[15px] leading-none transition-colors',
                      active
                        ? 'text-brand'
                        : 'text-muted group-hover:text-secondary',
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
    </aside>
  )
}
