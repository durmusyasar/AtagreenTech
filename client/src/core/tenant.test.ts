import { describe, expect, it } from 'vitest'
import { resolveTenantFromHost } from './tenant'

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
