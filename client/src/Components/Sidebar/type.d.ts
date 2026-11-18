export interface SidebarProps {
  collapsed: boolean
  /** Navbar toggle kullanıldığında opsiyonel (alt collapse şeridi yok). */
  onCollapse?: (collapsed: boolean) => void
}

/** API `GET /navigation/menus` menü öğesi. */
export interface NavigationMenuItem {
  code: string
  route: string
  icon: string
  translationKey: string
  children?: NavigationMenuItem[]
}

/** Servis grubu (General / StockService / UserService…). */
export interface NavigationMenuGroup {
  serviceName: string
  translationKey: string
  items: NavigationMenuItem[]
}

export interface NavigationMenusResponse {
  groups: NavigationMenuGroup[]
}

/** Sidebar / ⌘K ortak düzleştirilmiş satır. */
export interface FlatNavigationItem {
  code: string
  route: string
  icon: string
  translationKey: string
  serviceName: string
  serviceTranslationKey: string
}
