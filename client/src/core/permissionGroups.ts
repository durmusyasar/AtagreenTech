import type { PermissionItem } from '../types/rbac'

export interface PermissionModuleGroup {
  moduleKey: string
  items: PermissionItem[]
}

const KNOWN_MODULES = ['users', 'stock', 'audit'] as const

/** İzinleri `users.*` / `stock.*` gibi öneklerine göre gruplar. */
export function groupPermissionsByModule(permissions: PermissionItem[]): PermissionModuleGroup[] {
  const map = new Map<string, PermissionItem[]>()

  for (const perm of permissions) {
    const prefix = perm.name.split('.')[0] || 'other'
    const bucket = map.get(prefix) ?? []
    bucket.push(perm)
    map.set(prefix, bucket)
  }

  const orderedKeys = [
    ...KNOWN_MODULES.filter((key) => map.has(key)),
    ...[...map.keys()].filter((key) => !(KNOWN_MODULES as readonly string[]).includes(key)).sort(),
  ]

  return orderedKeys.map((moduleKey) => ({
    moduleKey,
    items: (map.get(moduleKey) ?? []).slice().sort((a, b) => a.name.localeCompare(b.name)),
  }))
}
