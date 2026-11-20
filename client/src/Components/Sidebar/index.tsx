import { useLocation, useNavigate } from 'react-router-dom'
import type { SidebarProps } from '@/Components/Sidebar/type'
import { useLanguage } from '@/core/context/useLanguage'
import { useNavigationMenus } from '@/hooks/useNavigationMenus'
import { useNavigationStore } from '@/stores/navigationStore'
import { resolveMenuIcon } from '@/utils/iconMapper'
import { resolveNavTitle } from '@/utils/resolveNavTitle'

/**
 * Sol navigasyon — z-30 elevation, canvas’tan border-r + lateral shadow ile ayrışır (#68).
 * `<lg`: fixed drawer; `lg+`: flex rail (232 / 68).
 */
export function Sidebar({ collapsed, mobileOpen = false, onNavigate }: SidebarProps) {
  const { pathname } = useLocation()
  const navigate = useNavigate()
  const { language, t } = useLanguage()
  const { isPending, isError } = useNavigationMenus()
  const groups = useNavigationStore((s) => s.groups)

  // Mobil drawer her zaman geniş etiket; desktop’ta collapse rail.
  const showLabels = mobileOpen || !collapsed

  const go = (route: string) => {
    navigate(route)
    onNavigate?.()
  }

  return (
    <aside
      className={[
        'z-30 flex flex-col border-r border-subtle bg-surface',
        'shadow-[2px_0_10px_rgba(15,23,42,0.04)] dark:shadow-[2px_0_14px_rgba(0,0,0,0.3)]',
        'transition-[width,transform] duration-200 ease-out',
        // Mobil: navbar altından fixed drawer
        'fixed bottom-0 left-0 top-16 w-[232px]',
        mobileOpen ? 'translate-x-0' : '-translate-x-full',
        // Desktop: akışta rail
        'lg:relative lg:top-auto lg:translate-x-0 lg:shrink-0',
        collapsed ? 'lg:w-[68px]' : 'lg:w-[232px]',
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

        {isError && groups.length === 0 && showLabels ? (
          <p className="px-2.5 text-xs text-muted">{t('menu.loadError')}</p>
        ) : null}

        {groups.map((group) => {
          const groupTitle = resolveNavTitle(group.translations, group.title, language)
          return (
            <div key={group.serviceName} className="space-y-0.5">
              {showLabels ? (
                <p className="px-2.5 pb-1 pt-0.5 text-[10px] font-medium uppercase tracking-[0.16em] text-muted/80">
                  {groupTitle}
                </p>
              ) : (
                <div
                  className="mx-auto mb-1 h-px w-6 border-t border-subtle/60"
                  aria-hidden
                />
              )}

              {group.items.map((item) => {
                const itemTitle = resolveNavTitle(item.translations, item.title, language)
                const active =
                  item.route === '/' ? pathname === '/' : pathname.startsWith(item.route)
                return (
                  <button
                    key={item.code}
                    type="button"
                    onClick={() => go(item.route)}
                    title={!showLabels ? itemTitle : undefined}
                    className={[
                      'group relative flex w-full appearance-none items-center gap-2.5 rounded-lg border-0 bg-transparent px-2.5 py-2 text-left text-[13px] outline-none transition-colors duration-150',
                      showLabels ? '' : 'justify-center px-2',
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
                        active ? 'text-brand' : 'text-muted group-hover:text-secondary',
                      ].join(' ')}
                    >
                      {resolveMenuIcon(item.icon)}
                    </span>
                    {showLabels ? <span className="truncate">{itemTitle}</span> : null}
                  </button>
                )
              })}
            </div>
          )
        })}
      </nav>
    </aside>
  )
}
