import { describe, expect, it } from 'vitest'
import { isExpired, readTokenClaims } from '@/core/jwt'

function token(payload: object): string {
  const encode = (value: object) =>
    btoa(String.fromCharCode(...new TextEncoder().encode(JSON.stringify(value))))
      .replaceAll('+', '-')
      .replaceAll('/', '_')
      .replaceAll('=', '')
  return `${encode({ alg: 'HS256', typ: 'JWT' })}.${encode(payload)}.signature`
}

describe('readTokenClaims', () => {
  it('reads subject, email, roles, permissions, tenant and expiry', () => {
    const claims = readTokenClaims(
      token({
        sub: 'u-1',
        email: 'büşra@atagreen.tech',
        role: ['Admin', 'Stock'],
        permission: ['users.read', 'stock.*'],
        tenant_id: 't-1',
        sid: 's-1',
        exp: 2000,
      }),
    )

    expect(claims).toEqual({
      subject: 'u-1',
      email: 'büşra@atagreen.tech',
      roles: ['Admin', 'Stock'],
      permissions: ['users.read', 'stock.*'],
      tenantId: 't-1',
      sessionId: 's-1',
      expiresAt: 2000,
    })
  })

  it('accepts a single role or permission string', () => {
    expect(readTokenClaims(token({ role: 'admin' }))?.roles).toEqual(['admin'])
    expect(readTokenClaims(token({ permission: 'stock.read' }))?.permissions).toEqual(['stock.read'])
  })

  it.each(['', 'abc', 'a.b', 'a.!!!.c', `${btoa('{}')}.${btoa('"text"')}.x`])('rejects malformed token %#', (value) => {
    expect(readTokenClaims(value)).toBeNull()
  })
})

describe('isExpired', () => {
  it('compares expiry with the current time', () => {
    const claims = readTokenClaims(token({ exp: 100 }))!
    expect(isExpired(claims, 99)).toBe(false)
    expect(isExpired(claims, 100)).toBe(true)
  })

  it('treats tokens without exp as not expired', () => {
    expect(isExpired(readTokenClaims(token({}))!, Number.MAX_SAFE_INTEGER)).toBe(false)
  })
})
