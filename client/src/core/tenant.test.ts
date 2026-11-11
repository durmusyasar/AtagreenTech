import { describe, expect, it } from 'vitest'
import { resolveTenantFromHost } from './tenant'

describe('resolveTenantFromHost', () => {
  it.each([
    ['acme.atagreen.tech', 'acme'],
    ['ACME.atagreen.tech', 'acme'],
    ['acme.atagreen.localhost', 'acme'],
  ])('reads the subdomain of %s', (host, tenant) => {
    expect(resolveTenantFromHost(host)).toBe(tenant)
  })

  it.each(['localhost', '127.0.0.1', '::1', 'atagreen.tech', 'www.atagreen.tech'])('has no tenant for %s', (host) => {
    expect(resolveTenantFromHost(host)).toBeNull()
  })
})
