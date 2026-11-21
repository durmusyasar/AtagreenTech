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
        'z-30 flex flex-col border-r border-subtle/80 bg-surface/90 backdrop-blur-md dark:border-white/[0.06]',
        'shadow-[2px_0_24px_-12px_rgba(15,23,42,0.18)] dark:shadow-[2px_0_28px_-10px_rgba(0,0,0,0.45)]',
        'transition-[width,transform] duration-200 ease-out',
        // Mobil: navbar altından fixed drawer
        'fixed bottom-0 left-0 top-16 w-[248px]',
        mobileOpen ? 'translate-x-0' : '-translate-x-full',
        // Desktop: akışta rail
        'lg:relative lg:top-auto lg:translate-x-0 lg:shrink-0',
        collapsed ? 'lg:w-[72px]' : 'lg:w-[248px]',
      ].join(' ')}
    >
      <nav
        className="custom-scrollbar flex-1 space-y-5 overflow-y-auto overflow-x-hidden px-2.5 py-4"
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
                <p className="px-3 pb-1.5 pt-0.5 text-[10px] font-semibold uppercase tracking-[0.14em] text-muted">
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
                      'group relative flex w-full appearance-none items-center gap-2.5 rounded-xl border-0 px-2.5 py-2 text-left text-[13px] outline-none transition-all duration-150',
                      showLabels ? '' : 'justify-center px-2',
                      active
                        ? 'bg-brand/[0.12] font-semibold text-brand shadow-[inset_0_0_0_1px_rgb(var(--brand-primary)/0.22)]'
                        : 'bg-transparent text-secondary hover:bg-elevated/70 hover:text-primary',
                    ].join(' ')}
                  >
                    <span
                      className={[
                        'inline-flex h-8 w-8 shrink-0 items-center justify-center rounded-lg text-[15px] leading-none transition-colors',
                        active
                          ? 'bg-brand/15 text-brand'
                          : 'bg-elevated/60 text-muted group-hover:bg-elevated group-hover:text-secondary',
                      ].join(' ')}
                    >
                      {resolveMenuIcon(item.icon)}
                    </span>
                    {showLabels ? <span className="truncate tracking-tight">{itemTitle}</span> : null}
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
