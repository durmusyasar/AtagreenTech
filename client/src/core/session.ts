import { DEFAULT_LANGUAGE, isSupportedLanguage, type Language } from './i18n/languages'
import { resolveTenantFromHost, TENANT_SELECTION_ENABLED } from './tenant'

const ACCESS_TOKEN_KEY = 'atagreen.accessToken'
const REFRESH_TOKEN_KEY = 'atagreen.refreshToken'
const EXPIRES_AT_KEY = 'atagreen.expiresAt'
const TENANT_KEY = 'atagreen.tenant'
const LANGUAGE_KEY = 'atagreen.language'

/** Sessions API'nin "current session" tespiti için (Issue #35). */
export const REFRESH_TOKEN_HEADER = 'X-Refresh-Token'

export interface SessionState {
  token: string | null
  refreshToken: string | null
  expiresAt: string | null
  tenant: string | null
  /**
   * Tenant adresin alt alan adından geliyorsa değiştirilemez ve `X-Tenant` başlığı gönderilmez. Adreste tenant yoksa
   * tenant yalnızca geliştirmede seçilebilir; derlenmiş pakette `null` kalır.
   */
  isTenantFromHost: boolean
  language: Language
}

export interface SignInSession {
  accessToken: string
  refreshToken: string
  expiresAt?: string | null
  tenant?: string
}

type Listener = () => void

const hostTenant = resolveTenantFromHost(window.location.hostname)
const listeners = new Set<Listener>()

// Token'lar sekme kapanınca silinsin diye sessionStorage'da tutulur; dil ve (geliştirmede) tenant kalıcıdır.
let state: SessionState = {
  token: read(sessionStorage, ACCESS_TOKEN_KEY),
  refreshToken: read(sessionStorage, REFRESH_TOKEN_KEY),
  expiresAt: read(sessionStorage, EXPIRES_AT_KEY),
  tenant:
    hostTenant ??
    (TENANT_SELECTION_ENABLED ? (read(localStorage, TENANT_KEY) ?? (import.meta.env.VITE_DEV_TENANT || null)) : null),
  isTenantFromHost: hostTenant !== null,
  language: readLanguage(),
}

/**
 * Oturum durumu: React tarafında `core/context` hook'ları, React dışında Axios interceptor'ları aynı kaynaktan okur.
 */
export const sessionStore = {
  getSnapshot: (): SessionState => state,

  subscribe(listener: Listener): () => void {
    listeners.add(listener)
    return () => listeners.delete(listener)
  },

  signIn({ accessToken, refreshToken, expiresAt, tenant }: SignInSession): void {
    write(sessionStorage, ACCESS_TOKEN_KEY, accessToken)
    write(sessionStorage, REFRESH_TOKEN_KEY, refreshToken)
    write(sessionStorage, EXPIRES_AT_KEY, expiresAt ?? null)

    const canSelect = TENANT_SELECTION_ENABLED && !state.isTenantFromHost
    const nextTenant = canSelect && tenant ? tenant.trim().toLowerCase() : state.tenant
    if (canSelect) write(localStorage, TENANT_KEY, nextTenant)

    update({
      token: accessToken,
      refreshToken,
      expiresAt: expiresAt ?? null,
      tenant: nextTenant,
    })
  },

  /** Access + refresh çifti (rotation sonrası). Tenant değişmez. */
  setTokens(accessToken: string, refreshToken: string, expiresAt?: string | null): void {
    write(sessionStorage, ACCESS_TOKEN_KEY, accessToken)
    write(sessionStorage, REFRESH_TOKEN_KEY, refreshToken)
    write(sessionStorage, EXPIRES_AT_KEY, expiresAt ?? null)
    update({ token: accessToken, refreshToken, expiresAt: expiresAt ?? null })
  },

  signOut(): void {
    write(sessionStorage, ACCESS_TOKEN_KEY, null)
    write(sessionStorage, REFRESH_TOKEN_KEY, null)
    write(sessionStorage, EXPIRES_AT_KEY, null)
    update({ token: null, refreshToken: null, expiresAt: null })
  },

  setLanguage(language: Language): void {
    write(localStorage, LANGUAGE_KEY, language)
    update({ language })
  },
}

function update(changes: Partial<SessionState>): void {
  state = { ...state, ...changes }
  listeners.forEach((listener) => listener())
}

function readLanguage(): Language {
  const stored = read(localStorage, LANGUAGE_KEY)
  return isSupportedLanguage(stored) ? stored : DEFAULT_LANGUAGE
}

// Depolama kapalıysa (ör. bazı gizli pencere modları) oturum yalnızca bellekte yaşar.
function read(storage: Storage, key: string): string | null {
  try {
    return storage.getItem(key)
  } catch {
    return null
  }
}

function write(storage: Storage, key: string, value: string | null): void {
  try {
    if (value === null) storage.removeItem(key)
    else storage.setItem(key, value)
  } catch {
    // Bellekteki durum yine de güncellenir.
  }
}
