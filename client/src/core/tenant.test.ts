import { describe, expect, it } from 'vitest'
import { resolveTenantFromHost, tenantAddressPattern } from './tenant'

describe('resolveTenantFromHost', () => {
  it.each([
    ['acme.omnisync.com', 'acme'],
    ['acme.omnisync.com.tr', 'acme'],
    ['acme.omnisync.localhost', 'acme'],
    ['ACME.OmniSync.com', 'acme'],
    ['acme.omnisync.com.', 'acme'],
  ])('reads the subdomain of %s', (host, tenant) => {
    expect(resolveTenantFromHost(host)).toBe(tenant)
  })

  it.each([
    'localhost',
    '127.0.0.1',
    '::1',
    'omnisync.com',
    'omnisync.com.tr',
    'www.omnisync.com',
    'api.acme.omnisync.com',
    'acme.other.com',
    'acme.atagreen.tech',
    'acme.localhost',
    'evilomnisync.com',
  ])('has no tenant for %s', (host) => {
    expect(resolveTenantFromHost(host)).toBeNull()
  })
})

describe('tenantAddressPattern', () => {
  it.each([
    ['omnisync.com', '', '<tenant>.omnisync.com'],
    ['www.omnisync.com.tr', '', '<tenant>.omnisync.com.tr'],
    ['omnisync.localhost', '3000', '<tenant>.omnisync.localhost:3000'],
    ['localhost', '3000', '<tenant>.omnisync.localhost:3000'],
    ['127.0.0.1', '8080', '<tenant>.omnisync.localhost:8080'],
    ['acme.atagreen.localhost', '3000', '<tenant>.omnisync.localhost:3000'],
    ['10.0.0.5', '', '<tenant>.omnisync.com'],
  ])('suggests the tenant address for %s:%s', (host, port, pattern) => {
    expect(tenantAddressPattern(host, port)).toBe(pattern)
  })
})
