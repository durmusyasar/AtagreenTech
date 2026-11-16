import type { ReactNode } from 'react'
import { usePermissions } from '../core/hooks/usePermissions'

interface PermissionGuardProps {
  permissions?: string | readonly string[]
  roles?: readonly string[]
  fallback?: ReactNode
  children: ReactNode
}

/**
 * Sayfa içi buton/bölümleri rol veya granüler izne göre koşullu render eder.
 * Örn: `<PermissionGuard permissions="stock.write"><Button>Ekle</Button></PermissionGuard>`
 */
export function PermissionGuard({
  permissions,
  roles,
  fallback = null,
  children,
}: PermissionGuardProps) {
  const access = usePermissions()
  const allowed = access.canAccess({ roles, permissions })
  return allowed ? children : fallback
}
