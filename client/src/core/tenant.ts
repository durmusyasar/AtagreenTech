const MINIMUM_HOST_PARTS_WITH_SUBDOMAIN = 3
const RESERVED_SUBDOMAINS = new Set(['www'])
const IPV4_PATTERN = /^\d{1,3}(\.\d{1,3}){3}$/

/**
 * Stok servisinin tenant çözümlemesiyle aynı kural: `acme.atagreen.tech` → `acme`. `localhost`, IP adresleri, iki
 * parçalı adresler ve `www` tenant içermez.
 */
export function resolveTenantFromHost(hostname: string): string | null {
  const host = hostname.toLowerCase()

  if (host === 'localhost' || IPV4_PATTERN.test(host) || host.includes(':')) return null

  const parts = host.split('.')
  if (parts.length < MINIMUM_HOST_PARTS_WITH_SUBDOMAIN) return null

  const subdomain = parts[0]
  return subdomain && !RESERVED_SUBDOMAINS.has(subdomain) ? subdomain : null
}
