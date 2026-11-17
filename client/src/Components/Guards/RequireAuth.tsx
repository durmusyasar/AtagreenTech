import { Navigate, useLocation } from 'react-router-dom'
import { useAuth } from '@/core/context/useAuth'
import { canAccess } from '@/core/permissions'
import type { RequireAuthProps } from '@/Components/Guards/type'

/**
 * Oturum yoksa `/login`'e, rol/izin yetmezse `/unauthorized`'a yönlendirir.
 * Üst layout için yalnızca oturum; alt rotalarda `permissions` / `roles` ile daraltılır.
 */
export function RequireAuth({ children, roles, permissions }: RequireAuthProps) {
  const { isAuthenticated, claims } = useAuth()
  const location = useLocation()

  if (!isAuthenticated) {
    return <Navigate to="/login" replace state={{ from: location.pathname + location.search }} />
  }

  if (roles || permissions) {
    const allowed = canAccess({
      userRoles: claims?.roles ?? [],
      userPermissions: claims?.permissions ?? [],
      roles,
      permissions,
    })
    if (!allowed) return <Navigate to="/unauthorized" replace />
  }

  return children
}
