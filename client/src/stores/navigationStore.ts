import { create } from 'zustand'
import type { FlatNavigationItem, NavigationMenuGroup, NavigationMenuItem } from '@/Components/Sidebar/type'

interface NavigationState {
  groups: NavigationMenuGroup[]
  flatItems: FlatNavigationItem[]
  setMenus: (groups: NavigationMenuGroup[]) => void
  clear: () => void
}

function flattenGroups(groups: NavigationMenuGroup[]): FlatNavigationItem[] {
  const result: FlatNavigationItem[] = []

  const walk = (items: NavigationMenuItem[], serviceName: string, serviceTranslationKey: string) => {
    for (const item of items) {
      result.push({
        code: item.code,
        route: item.route,
        icon: item.icon,
        translationKey: item.translationKey,
        serviceName,
        serviceTranslationKey,
      })
      if (item.children?.length) walk(item.children, serviceName, serviceTranslationKey)
    }
  }

  for (const group of groups) walk(group.items, group.serviceName, group.translationKey)
  return result
}

/** Oturum menüsü: Sidebar + ⌘K ortak kaynak. Logout'ta `clear` / session query clear ile sıfırlanır. */
export const useNavigationStore = create<NavigationState>((set) => ({
  groups: [],
  flatItems: [],
  setMenus: (groups) => set({ groups, flatItems: flattenGroups(groups) }),
  clear: () => set({ groups: [], flatItems: [] }),
}))
