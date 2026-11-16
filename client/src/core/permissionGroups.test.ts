import { describe, expect, it } from 'vitest'
import { groupPermissionsByModule } from './permissionGroups'

describe('groupPermissionsByModule', () => {
  it('groups by permission prefix and keeps known module order', () => {
    const groups = groupPermissionsByModule([
      { id: '1', name: 'stock.read', description: 's' },
      { id: '2', name: 'users.write', description: 'u' },
      { id: '3', name: 'audit.read', description: 'a' },
      { id: '4', name: 'users.read', description: 'u2' },
    ])

    expect(groups.map((g) => g.moduleKey)).toEqual(['users', 'stock', 'audit'])
    expect(groups[0].items.map((i) => i.name)).toEqual(['users.read', 'users.write'])
  })
})
