/** API yanıtı yokken veya hata durumunda kullanılan AtaGreen varsayılanları. */
export const DEFAULT_TENANT_THEME = {
  tenantId: 'default',
  subdomain: 'atagreen',
  name: 'AtaGreen Stok',
  primaryColor: '#2e7d32',
  logoUrl: '/favicon.svg',
  faviconUrl: '/favicon.svg',
} as const

/**
 * HEX birincil renkten Ant Design `colorPrimaryBg` / `colorPrimaryBgHover` tonlarını üretir.
 * Sabit #2e7d32 için önceki tema ile uyumlu açık yeşiller döner; diğer renklerde beyaza karıştırır.
 */
export function derivePrimarySurfaces(primaryHex: string): { colorPrimaryBg: string; colorPrimaryBgHover: string } {
  const rgb = parseHex(primaryHex)
  // Geçersiz veya varsayılan AtaGreen yeşili: önceki sabit açık tonlar (Material green-50/100).
  if (!rgb || primaryHex.trim().toLowerCase() === DEFAULT_TENANT_THEME.primaryColor) {
    return { colorPrimaryBg: '#e8f5e9', colorPrimaryBgHover: '#c8e6c9' }
  }

  return {
    colorPrimaryBg: toHex(mixWithWhite(rgb, 0.88)),
    colorPrimaryBgHover: toHex(mixWithWhite(rgb, 0.75)),
  }
}

export function applyCssPrimary(primaryHex: string): void {
  document.documentElement.style.setProperty('--primary-color', primaryHex)
}

export function applyFavicon(href: string): void {
  const head = document.head
  let link = head.querySelector<HTMLLinkElement>("link[rel='icon']")
  if (!link) {
    link = document.createElement('link')
    link.rel = 'icon'
    head.appendChild(link)
  }
  link.type = href.endsWith('.svg') ? 'image/svg+xml' : 'image/x-icon'
  link.href = href
}

export function applyDocumentTitle(brandName: string): void {
  document.title = brandName
}

function parseHex(hex: string): [number, number, number] | null {
  const normalized = hex.trim().replace(/^#/, '')
  if (!/^[0-9a-fA-F]{6}$/.test(normalized)) return null
  return [
    Number.parseInt(normalized.slice(0, 2), 16),
    Number.parseInt(normalized.slice(2, 4), 16),
    Number.parseInt(normalized.slice(4, 6), 16),
  ]
}

function mixWithWhite([r, g, b]: [number, number, number], whiteRatio: number): [number, number, number] {
  const keep = 1 - whiteRatio
  return [
    Math.round(r * keep + 255 * whiteRatio),
    Math.round(g * keep + 255 * whiteRatio),
    Math.round(b * keep + 255 * whiteRatio),
  ]
}

function toHex([r, g, b]: [number, number, number]): string {
  return `#${[r, g, b].map((n) => n.toString(16).padStart(2, '0')).join('')}`
}
