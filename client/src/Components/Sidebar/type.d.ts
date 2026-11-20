export interface SidebarProps {
  collapsed: boolean
  /** Navbar toggle kullanıldığında opsiyonel (alt collapse şeridi yok). */
  onCollapse?: (collapsed: boolean) => void
  /** `<lg` çekmece açık mı (#68). */
  mobileOpen?: boolean
  /** Mobilde rota seçilince çekmeceyi kapat. */
  onNavigate?: () => void
}

/** Dil kodu → görünen ad (API `translations` map). */
export type NavigationTranslations = Record<string, string>

/** API `GET /navigation/menus` menü öğesi. */
export interface NavigationMenuItem {
  code: string
  route: string
  icon: string
  title: string
  translations: NavigationTranslations
  children?: NavigationMenuItem[]
  requiredPermission?: string | null
}

/** Servis grubu (General / StockService / UserService…). */
export interface NavigationMenuGroup {
  moduleCode: string
  serviceName: string
  title: string
  translations: NavigationTranslations
  items: NavigationMenuItem[]
}

export interface NavigationMenusResponse {
  groups: NavigationMenuGroup[]
  /** Tenant'ta abone (görünür) modül kodları — #67 ModuleGuard. */
  subscribedModuleCodes?: string[]
}

/** Sidebar / ⌘K ortak düzlaştırilmiş satır. */
export interface FlatNavigationItem {
  code: string
  route: string
  icon: string
  title: string
  translations: NavigationTranslations
  serviceName: string
  moduleCode: string
  moduleTitle: string
  moduleTranslations: NavigationTranslations
}
