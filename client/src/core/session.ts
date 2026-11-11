import { DEFAULT_LANGUAGE, isSupportedLanguage, type Language } from './i18n/languages'
import { resolveTenantFromHost } from './tenant'

const TOKEN_KEY = 'atagreen.accessToken'
const TENANT_KEY = 'atagreen.tenant'
const LANGUAGE_KEY = 'atagreen.language'

export interface SessionState {
  token: string | null
  tenant: string | null
  /** Tenant adresin alt alan adından geliyorsa değiştirilemez ve `X-Tenant` başlığı gönderilmez. */
  isTenantFromHost: boolean
  language: Language
}

type Listener = () => void

const hostTenant = resolveTenantFromHost(window.location.hostname)
const listeners = new Set<Listener>()

// Token sekme kapanınca silinsin diye sessionStorage'da tutulur; tenant ve dil tercihi kalıcıdır.
let state: SessionState = {
  token: read(sessionStorage, TOKEN_KEY),
  tenant: hostTenant ?? read(localStorage, TENANT_KEY) ?? (import.meta.env.VITE_DEV_TENANT || null),
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

  signIn(token: string, tenant?: string): void {
    write(sessionStorage, TOKEN_KEY, token)
    const nextTenant = state.isTenantFromHost || !tenant ? state.tenant : tenant.trim().toLowerCase()
    if (!state.isTenantFromHost) write(localStorage, TENANT_KEY, nextTenant)
    update({ token, tenant: nextTenant })
  },

  signOut(): void {
    write(sessionStorage, TOKEN_KEY, null)
    update({ token: null })
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
