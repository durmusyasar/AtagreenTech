/**
 * Stok servisinin `TenantResolution:RootDomains` listesinin tüm ortamlardaki birleşimi; tek arayüz paketi hem canlıda
 * hem yerel Docker'da sunulduğu için yerel domain de listededir.
 */
const ROOT_DOMAINS = ['omnisync.com', 'omnisync.com.tr', 'omnisync.localhost'] as const
const LOCAL_ROOT_DOMAIN = 'omnisync.localhost'

const RESERVED_SUBDOMAINS = new Set(['www'])
const LOOPBACK_HOSTS = new Set(['localhost', '127.0.0.1', '[::1]', '::1'])

/**
 * Adreste tenant yoksa tenant arayüzden seçilip `X-Tenant` başlığıyla gönderilebilir mi. Stok servisi bu başlığı
 * yalnızca Development'ta kabul ettiğinden seçim yalnızca geliştirme sunucusunda (`npm run dev`) açıktır; derlenmiş
 * pakette tenant her zaman adresten gelir.
 */
export const TENANT_SELECTION_ENABLED = import.meta.env.DEV

/**
 * Stok servisinin tenant çözümlemesiyle aynı kural: `acme.omnisync.com` → `acme`. Tenant, kök domain'in hemen
 * altındaki tek etikettir; kök domain'in kendisi, `www`, çok seviyeli alt alan adları (`api.acme.omnisync.com`) ve
 * listede olmayan adresler (`localhost`, IP adresleri) tenant içermez.
 */
export function resolveTenantFromHost(hostname: string): string | null {
  const host = normalize(hostname)
  const rootDomain = findRootDomain(host)
  if (!rootDomain || host === rootDomain) return null

  const subdomain = host.slice(0, -(rootDomain.length + 1))
  return subdomain.includes('.') || RESERVED_SUBDOMAINS.has(subdomain) ? null : subdomain
}

/**
 * Tenant içermeyen bir adreste kullanıcıya gösterilecek tenant adresi kalıbı: kök domain'deyse aynı domain,
 * yerel makinedeyse yerel domain, aksi halde birincil domain (`<tenant>.omnisync.com`).
 */
export function tenantAddressPattern(hostname: string, port = ''): string {
  const host = normalize(hostname)
  const rootDomain = findRootDomain(host) ?? (isLocal(host) ? LOCAL_ROOT_DOMAIN : ROOT_DOMAINS[0])
  return `<tenant>.${rootDomain}${port ? `:${port}` : ''}`
}

function normalize(hostname: string): string {
  return hostname.toLowerCase().replace(/\.$/, '')
}

/** Adresin kendisi veya üst domain'i olan en uzun kök domain (`omnisync.com.tr`, `omnisync.com`'dan önce gelir). */
function findRootDomain(host: string): string | undefined {
  return ROOT_DOMAINS.filter((domain) => host === domain || host.endsWith(`.${domain}`)).sort(
    (a, b) => b.length - a.length,
  )[0]
}

function isLocal(host: string): boolean {
  return LOOPBACK_HOSTS.has(host) || host.endsWith('.localhost')
}
