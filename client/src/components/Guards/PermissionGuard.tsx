import { usePermissions } from '@/core/hooks/usePermissions'
import type { PermissionGuardProps } from '@/Components/Guards/type'

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
