import type { ReactNode } from 'react'

export interface RequireAuthProps {
  children: ReactNode
  /** En az biri gerekli (büyük/küçük harf duyarsız). */
  roles?: readonly string[]
  /** En az biri gerekli; `stock.*` / `*` desteklenir. */
  permissions?: string | readonly string[]
}

export interface PermissionGuardProps {
  permissions?: string | readonly string[]
  roles?: readonly string[]
  fallback?: ReactNode
  children: ReactNode
}

export interface RequireAdminProps {
  children: ReactNode
}
