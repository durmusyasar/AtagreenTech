import axios, { isAxiosError, isCancel, type InternalAxiosRequestConfig } from 'axios'
import { translate } from '@/core/i18n/messages'
import { REFRESH_TOKEN_HEADER, sessionStore } from '@/core/session'
import { toApiError } from '@/core/api/apiError'
import { delay, NETWORK_RETRY_DELAY_MS, nextRetryConfig, shouldRetryNetworkError } from '@/core/api/networkRetry'
import { notify } from '@/core/api/notifier'
import { refreshAccessToken } from '@/core/api/refreshAccessToken'
import { useNavigationStore } from '@/stores/navigationStore'

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
    url.includes('/auth/accept-invite') ||
    url.includes('/auth/verify-email') ||
    url.includes('/auth/resend-verification') ||
    url.includes('/auth/forgot-password') ||
    url.includes('/auth/reset-password')
  )
}

/**
 * Oturum açıkken şifre doğrulayan uçlar. 401 burada "yanlış şifre" olabilir; refresh/signOut yapma.
 * (Kalıcı çözüm: bu uçlar yanlış şifrede 400 döner; bu liste savunma katmanı.)
 */
function isPasswordConfirmationUrl(url: string | undefined): boolean {
  if (!url) return false
  return url.includes('/auth/mfa/disable') || url.includes('/auth/change-password')
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
      !isPasswordConfirmationUrl(originalRequest.url) &&
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
      // Şifre onay uçlarında 401 = yanlış şifre; oturumu düşürme.
      if (
        token &&
        !isAuthBootstrapUrl(originalRequest?.url) &&
        !isPasswordConfirmationUrl(originalRequest?.url)
      ) {
        sessionStore.signOut()
        if (!(isAxiosError(error) && error.config?.skipErrorNotification)) {
          notify('warning', translate(language, 'error.unauthorized'), 'unauthorized')
        }
      }
    } else if (
      apiError.status === 403 &&
      apiError.code === 'MODULE_NOT_SUBSCRIBED'
    ) {
      // Tenant aboneliği kapandı (#67): store'dan modülü düşür → ModuleGuard NotSubscribed gösterir.
      const raw = isAxiosError(error) ? error.response?.data : null
      const moduleCode =
        raw && typeof raw === 'object' && 'moduleCode' in raw && typeof raw.moduleCode === 'string'
          ? raw.moduleCode
          : null
      if (moduleCode) {
        useNavigationStore.getState().revokeModule(moduleCode)
      }
      if (!(isAxiosError(error) && error.config?.skipErrorNotification)) {
        const labelKey = moduleCode ? (`modules.${moduleCode}` as const) : null
        const label =
          labelKey && moduleCode
            ? translate(language, labelKey, [])
            : moduleCode ?? 'module'
        notify(
          'warning',
          translate(language, 'moduleUnavailable.description', [label]),
          'MODULE_NOT_SUBSCRIBED',
        )
      }
    } else if (!(isAxiosError(error) && error.config?.skipErrorNotification)) {
      if (!(apiError.code === 'network_error' && !isOnline)) {
        notify('error', apiError.message, apiError.resourceKey ?? apiError.code)
      }
    }

    return Promise.reject(apiError)
  },
)
