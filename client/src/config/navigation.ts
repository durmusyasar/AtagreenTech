import type { ReactNode } from 'react'
import type { MessageKey } from '@/core/i18n/messages'

export type NavSection = 'main' | 'admin'

export interface NavItemConfig {
  path: string
  label: MessageKey
  section: NavSection
  /** Verilirse kullanıcının rollerinden en az biri gerekir. */
  roles?: readonly string[]
  /** Verilirse kullanıcının izinlerinden en az biri gerekir (`stock.*` / `*` destekli). */
  permissions?: string | readonly string[]
  icon: ReactNode
}

/**
 * Sol menü + komut paleti ortak kaynak. Görünürlük `canAccess` ile filtrelenir.
 * Audit log UI henüz yok; admin audit girişi kasıtlı olarak eklenmedi.
 */
export function createNavigationItems(icons: {
  dashboard: ReactNode
  units: ReactNode
  categories: ReactNode
  products: ReactNode
  warehouses: ReactNode
  movements: ReactNode
  balances: ReactNode
  security: ReactNode
  users: ReactNode
  roles: ReactNode
}): NavItemConfig[] {
  return [
    { path: '/', label: 'menu.dashboard', section: 'main', icon: icons.dashboard },
    {
      path: '/units-of-measure',
      label: 'menu.unitsOfMeasure',
      section: 'main',
      permissions: 'stock.read',
      icon: icons.units,
    },
    {
      path: '/categories',
      label: 'menu.categories',
      section: 'main',
      permissions: 'stock.read',
      icon: icons.categories,
    },
    {
      path: '/products',
      label: 'menu.products',
      section: 'main',
      permissions: 'stock.read',
      icon: icons.products,
    },
    {
      path: '/warehouses',
      label: 'menu.warehouses',
      section: 'main',
      permissions: 'stock.read',
      icon: icons.warehouses,
    },
    {
      path: '/stock-movements',
      label: 'menu.stockMovements',
      section: 'main',
      permissions: 'stock.read',
      icon: icons.movements,
    },
    {
      path: '/stock-balances',
      label: 'menu.stockBalances',
      section: 'main',
      permissions: 'stock.read',
      icon: icons.balances,
    },
    { path: '/account/security', label: 'menu.security', section: 'main', icon: icons.security },
    {
      path: '/admin/users',
      label: 'menu.users',
      section: 'admin',
      roles: ['Admin'],
      permissions: 'users.read',
      icon: icons.users,
    },
    {
      path: '/admin/roles',
      label: 'menu.roles',
      section: 'admin',
      roles: ['Admin'],
      icon: icons.roles,
    },
  ]
}
