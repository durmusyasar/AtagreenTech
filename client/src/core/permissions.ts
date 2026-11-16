/**
 * Backend `PermissionMatcher` ile uyumlu istemci izin kontrolü:
 * somut izin (`stock.read`), alan jokerı (`stock.*`), sistem jokerı (`*`).
 */

/** Kullanıcının sahip olduğu izinler istenen izni karşılar mı? */
export function hasPermission(granted: readonly string[], required: string): boolean {
  const need = required.trim()
  if (!need) return true

  for (const raw of granted) {
    if (!raw?.trim()) continue
    const grant = raw.trim()

    if (grant === '*') return true
    if (grant.toLowerCase() === need.toLowerCase()) return true

    if (grant.endsWith('.*')) {
      const prefix = grant.slice(0, -1) // "stock."
      if (need.toLowerCase().startsWith(prefix.toLowerCase())) return true
    }
  }

  return false
}

/** Hedef izinlerden herhangi biri karşılanıyorsa true (OR). */
export function hasAnyPermission(
  granted: readonly string[],
  required?: string | readonly string[],
): boolean {
  if (required === undefined) return true
  const targets = typeof required === 'string' ? [required] : required
  if (targets.length === 0) return true
  return targets.some((target) => hasPermission(granted, target))
}

/** Rol listesinde (büyük/küçük harf duyarsız) eşleşme. */
export function hasAnyRole(userRoles: readonly string[], requiredRoles?: readonly string[]): boolean {
  if (!requiredRoles || requiredRoles.length === 0) return true
  const normalized = new Set(userRoles.map((r) => r.toLowerCase()))
  return requiredRoles.some((role) => normalized.has(role.toLowerCase()))
}

/** Rol ve izin koşullarının ikisi de sağlanmalı (AND). */
export function canAccess(options: {
  userRoles: readonly string[]
  userPermissions: readonly string[]
  roles?: readonly string[]
  permissions?: string | readonly string[]
}): boolean {
  return (
    hasAnyRole(options.userRoles, options.roles) &&
    hasAnyPermission(options.userPermissions, options.permissions)
  )
}
