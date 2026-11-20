import type { ReactNode } from 'react'
import type { ModuleCode } from '@core/modules/types'

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

export interface ModuleGuardProps {
  moduleCode: ModuleCode
  /** true ise abonelik kontrolü atlanır (GENERAL / IAM / SYSTEM). */
  isCore?: boolean
  children: ReactNode
}
