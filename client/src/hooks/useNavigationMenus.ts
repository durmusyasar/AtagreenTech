import { useEffect } from 'react'
import { useAuth } from '@/core/context/useAuth'
import { useAppQuery } from '@/core/hooks/useAppQuery'
import { navigationQueries } from '@/services/navigationService'
import { useNavigationStore } from '@/stores/navigationStore'

/**
 * Oturum açıkken `GET /navigation/menus` çeker; sonucu `navigationStore`'a yazar.
 * Logout'ta store boşaltılır (query cache de `clearOnSessionChange` ile temizlenir).
 */
export function useNavigationMenus() {
  const { isAuthenticated } = useAuth()
  const setMenus = useNavigationStore((s) => s.setMenus)
  const clear = useNavigationStore((s) => s.clear)

  const query = useAppQuery({
    ...navigationQueries.menus(),
    enabled: isAuthenticated,
  })

  useEffect(() => {
    if (query.data?.groups) {
      setMenus(query.data.groups, query.data.subscribedModuleCodes ?? [])
    }
  }, [query.data, setMenus])

  useEffect(() => {
    if (!isAuthenticated) clear()
  }, [isAuthenticated, clear])

  return query
}
