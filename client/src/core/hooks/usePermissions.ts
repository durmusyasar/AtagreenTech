import { useMemo } from 'react'
import { useAuth } from '@/core/context/useAuth'
import { canAccess, hasAnyPermission, hasAnyRole, hasPermission } from '@/core/permissions'

export interface PermissionsApi {
  roles: string[]
  permissions: string[]
  can: (required: string) => boolean
  canAny: (required: string | readonly string[]) => boolean
  hasRole: (...roles: string[]) => boolean
  canAccess: (options: { roles?: readonly string[]; permissions?: string | readonly string[] }) => boolean
}

/** JWT claim'lerinden rol/izin API'si (menü, guard, buton görünürlüğü). */
export function usePermissions(): PermissionsApi {
  const { claims } = useAuth()

  return useMemo(() => {
    const roles = claims?.roles ?? []
    const permissions = claims?.permissions ?? []
    return {
      roles,
      permissions,
      can: (required: string) => hasPermission(permissions, required),
      canAny: (required: string | readonly string[]) => hasAnyPermission(permissions, required),
      hasRole: (...needed: string[]) => hasAnyRole(roles, needed),
      canAccess: (options: { roles?: readonly string[]; permissions?: string | readonly string[] }) =>
        canAccess({
          userRoles: roles,
          userPermissions: permissions,
          roles: options.roles,
          permissions: options.permissions,
        }),
    }
  }, [claims])
}
