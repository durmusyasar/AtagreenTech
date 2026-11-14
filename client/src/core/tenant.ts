/**
 * Stok servisinin `TenantResolution:RootDomains` listesinin tüm ortamlardaki birleşimi; tek arayüz paketi hem canlıda
 * hem yerel Docker'da sunulduğu için yerel domain de listededir.
 */
const ROOT_DOMAINS = ['omnisync.com', 'omnisync.com.tr', 'omnisync.localhost'] as const

const RESERVED_SUBDOMAINS = new Set(['www'])

/**
 * Stok servisinin tenant çözümlemesiyle aynı kural: `acme.omnisync.com` → `acme`. Tenant, kök domain'in hemen
 * altındaki tek etikettir; kök domain'in kendisi, `www`, çok seviyeli alt alan adları (`api.acme.omnisync.com`) ve
 * listede olmayan adresler (`localhost`, IP adresleri) tenant içermez.
 */
export function resolveTenantFromHost(hostname: string): string | null {
  const host = hostname.toLowerCase().replace(/\.$/, '')

  const rootDomain = ROOT_DOMAINS.filter((domain) => host.endsWith(`.${domain}`)).sort((a, b) => b.length - a.length)[0]
  if (!rootDomain) return null

  const subdomain = host.slice(0, -(rootDomain.length + 1))
  return subdomain.includes('.') || RESERVED_SUBDOMAINS.has(subdomain) ? null : subdomain
}
