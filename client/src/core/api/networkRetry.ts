import type { AxiosError, InternalAxiosRequestConfig, Method } from 'axios'

/** Ağ hatasında güvenli (idempotent) istekler için en fazla kaç kez yeniden deneneceği. */
export const NETWORK_RETRY_LIMIT = 2

/** Yeniden denemeler arasındaki bekleme (ms). */
export const NETWORK_RETRY_DELAY_MS = 1_500

const IDEMPOTENT_METHODS = new Set(['get', 'head', 'options'])

declare module 'axios' {
  interface AxiosRequestConfig {
    /** Ağ hatası yeniden deneme sayacı; interceptor doldurur. */
    networkRetryCount?: number
  }
}

/**
 * Yanıtsız (network) hatalarda GET/HEAD/OPTIONS için yeniden deneme uygun mu?
 * POST/PUT/PATCH/DELETE stok hareketlerinde çift işlem riski doğurduğu için denenmez.
 * Tarayıcı çevrimdışıysa bekletmeden bırakılır (OfflineBanner zaten görünür).
 */
export function shouldRetryNetworkError(error: AxiosError, isOnline = true): boolean {
  if (error.response) return false
  if (!isOnline) return false
  if (!error.config) return false

  const method = (error.config.method ?? 'get').toLowerCase() as Lowercase<Method>
  if (!IDEMPOTENT_METHODS.has(method)) return false

  const attempts = error.config.networkRetryCount ?? 0
  return attempts < NETWORK_RETRY_LIMIT
}

export function nextRetryConfig(config: InternalAxiosRequestConfig): InternalAxiosRequestConfig {
  return {
    ...config,
    networkRetryCount: (config.networkRetryCount ?? 0) + 1,
  }
}

export function delay(ms: number): Promise<void> {
  return new Promise((resolve) => {
    setTimeout(resolve, ms)
  })
}
