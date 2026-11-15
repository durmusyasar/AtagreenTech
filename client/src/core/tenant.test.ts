import { describe, expect, it } from 'vitest'
import { resolveTenantFromHost, tenantAddressPattern } from './tenant'

describe('resolveTenantFromHost', () => {
  it.each([
    ['atagreen.omnisync.com', 'atagreen'],
    ['atagreen.omnisync.com.tr', 'atagreen'],
    ['atagreen.omnisync.localhost', 'atagreen'],
    ['ATAGREEN.OmniSync.com', 'atagreen'],
    ['atagreen.omnisync.com.', 'atagreen'],
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
    'api.atagreen.omnisync.com',
    'atagreen.other.com',
    'shop.atagreen.tech',
    'atagreen.localhost',
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
    ['shop.atagreen.localhost', '3000', '<tenant>.omnisync.localhost:3000'],
    ['10.0.0.5', '', '<tenant>.omnisync.com'],
  ])('suggests the tenant address for %s:%s', (host, port, pattern) => {
    expect(tenantAddressPattern(host, port)).toBe(pattern)
  })
})
