import axios, { isAxiosError, isCancel, type InternalAxiosRequestConfig } from 'axios'
import { translate } from '../i18n/messages'
import { REFRESH_TOKEN_HEADER, sessionStore } from '../session'
import { toApiError } from './apiError'
import { delay, NETWORK_RETRY_DELAY_MS, nextRetryConfig, shouldRetryNetworkError } from './networkRetry'
import { notify } from './notifier'
import { refreshAccessToken } from './refreshAccessToken'

declare module 'axios' {
  interface AxiosRequestConfig {
    /** Hatayı çağıran kendisi gösterecekse (ör. form alanlarında) global bildirimi kapatır; 401 yine işlenir. */
    skipErrorNotification?: boolean
    /** Refresh sonrası yeniden denenen isteklerde tekrar refresh tetiklenmesin. */
    _retry?: boolean
  }
}

/**
 * Stok / kimlik API istemcisi. API aynı origin'den (`/api/v1`) çağrılır: geliştirmede Vite, Docker'da nginx.
 *
 * 401'de tek uçuşluk refresh + kuyruk (Issue #27 rotation / reuse detection ile uyumlu).
 */
export const axiosClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || '/api/v1',
  timeout: 30_000,
  headers: { Accept: 'application/json' },
})

let isRefreshing = false
let failedQueue: Array<{
  resolve: (token: string) => void
  reject: (error: unknown) => void
}> = []

function processQueue(error: unknown, token: string | null = null): void {
  failedQueue.forEach((entry) => {
    if (error) entry.reject(error)
    else if (token) entry.resolve(token)
  })
  failedQueue = []
}

function isAuthBootstrapUrl(url: string | undefined): boolean {
  if (!url) return false
  return (
    url.includes('/auth/login') ||
    url.includes('/auth/refresh') ||
    url.includes('/auth/logout') ||
    url.includes('/auth/accept-invite')
  )
}

axiosClient.interceptors.request.use((config) => {
  const { token, refreshToken, tenant, isTenantFromHost, language } = sessionStore.getSnapshot()

  if (token) config.headers.Authorization = `Bearer ${token}`

  // Issue #35: sessions API "current" oturumu refresh token başlığından tanır.
  if (refreshToken) config.headers[REFRESH_TOKEN_HEADER] = refreshToken

  // Tenant'ın alt alan adı API'ye Host başlığıyla ulaşır. Adreste tenant yoksa yalnızca geliştirmede seçilen tenant
  // X-Tenant ile gönderilir; derlenmiş pakette bu durumda tenant hiç olmaz (bkz. TENANT_SELECTION_ENABLED).
  if (tenant && !isTenantFromHost) config.headers['X-Tenant'] = tenant

  config.headers['Accept-Language'] = language
  return config
})

axiosClient.interceptors.response.use(
  (response) => response,
  async (error: unknown) => {
    if (isCancel(error)) return Promise.reject(error)

    const isOnline = typeof navigator === 'undefined' ? true : navigator.onLine

    if (isAxiosError(error) && error.config && shouldRetryNetworkError(error, isOnline)) {
      const retryConfig = nextRetryConfig(error.config)
      await delay(NETWORK_RETRY_DELAY_MS)
      return axiosClient.request(retryConfig)
    }

    const { language, token, refreshToken } = sessionStore.getSnapshot()
    const apiError = toApiError(error, language)
    const originalRequest = isAxiosError(error)
      ? (error.config as InternalAxiosRequestConfig | undefined)
      : undefined

    if (
      apiError.status === 401 &&
      originalRequest &&
      !originalRequest._retry &&
      !isAuthBootstrapUrl(originalRequest.url) &&
      refreshToken
    ) {
      if (isRefreshing) {
        return new Promise<string>((resolve, reject) => {
          failedQueue.push({ resolve, reject })
        }).then((newToken) => {
          originalRequest.headers.Authorization = `Bearer ${newToken}`
          return axiosClient.request(originalRequest)
        })
      }

      originalRequest._retry = true
      isRefreshing = true

      try {
        const tokens = await refreshAccessToken(refreshToken)
        processQueue(null, tokens.accessToken)
        originalRequest.headers.Authorization = `Bearer ${tokens.accessToken}`
        return axiosClient.request(originalRequest)
      } catch (refreshError) {
        processQueue(refreshError, null)
        sessionStore.signOut()
        if (!originalRequest.skipErrorNotification) {
          notify('warning', translate(language, 'error.unauthorized'), 'unauthorized')
        }
        return Promise.reject(toApiError(refreshError, language))
      } finally {
        isRefreshing = false
      }
    }

    if (apiError.status === 401) {
      // Refresh yok / login-refresh uçları / retry sonrası: oturumu temizle (login ekranı MFA 401'inde token olmayabilir).
      if (token && !isAuthBootstrapUrl(originalRequest?.url)) {
        sessionStore.signOut()
        if (!(isAxiosError(error) && error.config?.skipErrorNotification)) {
          notify('warning', translate(language, 'error.unauthorized'), 'unauthorized')
        }
      }
    } else if (!(isAxiosError(error) && error.config?.skipErrorNotification)) {
      if (!(apiError.code === 'network_error' && !isOnline)) {
        notify('error', apiError.message, apiError.resourceKey ?? apiError.code)
      }
    }

    return Promise.reject(apiError)
  },
)
