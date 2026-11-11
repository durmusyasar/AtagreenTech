export interface TokenClaims {
  subject: string | null
  email: string | null
  roles: string[]
  tenantId: string | null
  /** Bitiş zamanı (Unix saniye); token'da yoksa `null`. */
  expiresAt: number | null
}

const ROLE_CLAIMS = ['role', 'roles', 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role']

/**
 * Token'ın içeriğini arayüzde göstermek ve süresini kontrol etmek için okur. İmza doğrulanmaz; bunu her istekte API
 * yapar. Biçimi bozuk token'larda `null` döner.
 */
export function readTokenClaims(token: string): TokenClaims | null {
  const parts = token.trim().split('.')
  if (parts.length !== 3) return null

  try {
    const payload: unknown = JSON.parse(decodeBase64Url(parts[1]))
    if (typeof payload !== 'object' || payload === null) return null

    const claims = payload as Record<string, unknown>
    return {
      subject: asString(claims.sub),
      email: asString(claims.email),
      roles: ROLE_CLAIMS.flatMap((name) => asStringList(claims[name])),
      tenantId: asString(claims.tenant_id),
      expiresAt: typeof claims.exp === 'number' ? claims.exp : null,
    }
  } catch {
    return null
  }
}

export function isExpired(claims: TokenClaims, nowSeconds = Date.now() / 1000): boolean {
  return claims.expiresAt !== null && claims.expiresAt <= nowSeconds
}

function decodeBase64Url(value: string): string {
  const base64 = value.replace(/-/g, '+').replace(/_/g, '/').padEnd(Math.ceil(value.length / 4) * 4, '=')
  const bytes = Uint8Array.from(atob(base64), (char) => char.charCodeAt(0))
  return new TextDecoder().decode(bytes)
}

function asString(value: unknown): string | null {
  return typeof value === 'string' && value.length > 0 ? value : null
}

function asStringList(value: unknown): string[] {
  if (Array.isArray(value)) return value.filter((item): item is string => typeof item === 'string')
  return typeof value === 'string' ? [value] : []
}
