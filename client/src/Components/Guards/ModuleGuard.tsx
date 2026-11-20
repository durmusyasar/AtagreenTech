import { PageLoader } from '@core/routing/PageLoader'
import { ModuleNotSubscribedPage } from '@/routes/module-not-subscribed'
import { useNavigationStore } from '@/stores/navigationStore'
import type { ModuleGuardProps } from '@/Components/Guards/type'

/**
 * Tenant abonelik koruması (#67).
 * `isCore` → bypass. Menü yüklenmeden NotSubscribed flash etmez.
 * Children (lazy) yalnızca abone iken mount edilir → chunk indirilmez.
 */
export function ModuleGuard({ moduleCode, isCore = false, children }: ModuleGuardProps) {
  const isMenusLoaded = useNavigationStore((s) => s.isMenusLoaded)
  const subscribedModuleCodes = useNavigationStore((s) => s.subscribedModuleCodes)

  if (isCore) return children

  if (!isMenusLoaded) return <PageLoader />

  const subscribed = subscribedModuleCodes.some(
    (code) => code.localeCompare(moduleCode, undefined, { sensitivity: 'accent' }) === 0,
  )

  if (!subscribed) return <ModuleNotSubscribedPage moduleCode={moduleCode} />

  return children
}
