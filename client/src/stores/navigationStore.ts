import { create } from 'zustand'
import type { FlatNavigationItem, NavigationMenuGroup, NavigationMenuItem } from '@/Components/Sidebar/type'

interface NavigationState {
  groups: NavigationMenuGroup[]
  flatItems: FlatNavigationItem[]
  /** Tenant abone modül kodları (#66 / #67). */
  subscribedModuleCodes: string[]
  /** Menü API yanıtı yazıldı mı — ModuleGuard flash önleme. */
  isMenusLoaded: boolean
  setMenus: (groups: NavigationMenuGroup[], subscribedModuleCodes?: string[]) => void
  /** Abonelik kodlarını anında güncelle (403 MODULE_NOT_SUBSCRIBED). */
  revokeModule: (moduleCode: string) => void
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
  subscribedModuleCodes: [],
  isMenusLoaded: false,
  setMenus: (groups, subscribedModuleCodes = []) =>
    set({
      groups,
      flatItems: flattenGroups(groups),
      subscribedModuleCodes,
      isMenusLoaded: true,
    }),
  revokeModule: (moduleCode) =>
    set((state) => ({
      subscribedModuleCodes: state.subscribedModuleCodes.filter(
        (code) => code.localeCompare(moduleCode, undefined, { sensitivity: 'accent' }) !== 0,
      ),
      groups: state.groups.filter(
        (g) => g.moduleCode.localeCompare(moduleCode, undefined, { sensitivity: 'accent' }) !== 0,
      ),
      flatItems: state.flatItems.filter(
        (i) => i.moduleCode.localeCompare(moduleCode, undefined, { sensitivity: 'accent' }) !== 0,
      ),
    })),
  clear: () =>
    set({
      groups: [],
      flatItems: [],
      subscribedModuleCodes: [],
      isMenusLoaded: false,
    }),
}))
