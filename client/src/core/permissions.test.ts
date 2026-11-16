import { describe, expect, it } from 'vitest'
import { canAccess, hasAnyPermission, hasAnyRole, hasPermission } from './permissions'

describe('hasPermission', () => {
  it('matches exact, module wildcard, and system wildcard', () => {
    expect(hasPermission(['stock.read'], 'stock.read')).toBe(true)
    expect(hasPermission(['stock.*'], 'stock.write')).toBe(true)
    expect(hasPermission(['*'], 'users.delete')).toBe(true)
    expect(hasPermission(['stock.read'], 'stock.write')).toBe(false)
    expect(hasPermission(['users.*'], 'stock.read')).toBe(false)
  })

  it('is case-insensitive for exact matches', () => {
    expect(hasPermission(['Stock.Read'], 'stock.read')).toBe(true)
  })
})

describe('hasAnyPermission / hasAnyRole / canAccess', () => {
  it('ORs permission targets and ANDs role+permission requirements', () => {
    expect(hasAnyPermission(['users.read'], ['stock.write', 'users.read'])).toBe(true)
    expect(hasAnyRole(['Manager'], ['Admin', 'Manager'])).toBe(true)
    expect(
      canAccess({
        userRoles: ['Admin'],
        userPermissions: ['users.read'],
        roles: ['Admin'],
        permissions: 'users.read',
      }),
    ).toBe(true)
    expect(
      canAccess({
        userRoles: ['Manager'],
        userPermissions: ['users.read'],
        roles: ['Admin'],
        permissions: 'users.read',
      }),
    ).toBe(false)
  })
})
