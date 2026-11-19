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

  const walk = (
    items: NavigationMenuItem[],
    serviceName: string,
    moduleCode: string,
    moduleTitle: string,
    moduleTranslations: NavigationMenuGroup['translations'],
  ) => {
    for (const item of items) {
      result.push({
        code: item.code,
        route: item.route,
        icon: item.icon,
        title: item.title,
        translations: item.translations ?? {},
        serviceName,
        moduleCode,
        moduleTitle,
        moduleTranslations,
      })
      if (item.children?.length) {
        walk(item.children, serviceName, moduleCode, moduleTitle, moduleTranslations)
      }
    }
  }

  for (const group of groups) {
    walk(
      group.items,
      group.serviceName,
      group.moduleCode,
      group.title,
      group.translations ?? {},
    )
  }
  return result
}

/** Oturum menüsü: Sidebar + ⌘K ortak kaynak. Logout'ta `clear` / session query clear ile sıfırlanır. */
export const useNavigationStore = create<NavigationState>((set) => ({
  groups: [],
  flatItems: [],
  setMenus: (groups) => set({ groups, flatItems: flattenGroups(groups) }),
  clear: () => set({ groups: [], flatItems: [] }),
}))
