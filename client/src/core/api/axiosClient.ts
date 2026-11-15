import axios, { isAxiosError, isCancel } from 'axios'
import { translate } from '../i18n/messages'
import { sessionStore } from '../session'
import { toApiError } from './apiError'
import { delay, NETWORK_RETRY_DELAY_MS, nextRetryConfig, shouldRetryNetworkError } from './networkRetry'
import { notify } from './notifier'

declare module 'axios' {
  interface AxiosRequestConfig {
    /** Hatayı çağıran kendisi gösterecekse (ör. form alanlarında) global bildirimi kapatır; 401 yine işlenir. */
    skipErrorNotification?: boolean
  }
}

/**
 * Stok servisi istemcisi. API aynı origin'den (`/api/v1`) çağrılır: geliştirmede Vite, Docker'da nginx istekleri
 * servise aktarır.
 *
 * Yanıtsız ağ hatalarında güvenli (GET/HEAD/OPTIONS) istekler sınırlı kez yeniden denenir; bildirim yalnızca
 * son başarısızlıkta gösterilir. Çevrimdışıyken yeniden deneme yapılmaz (`OfflineBanner` yeterlidir).
 */
export const axiosClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || '/api/v1',
  timeout: 30_000,
  headers: { Accept: 'application/json' },
})

axiosClient.interceptors.request.use((config) => {
  const { token, tenant, isTenantFromHost, language } = sessionStore.getSnapshot()

  if (token) config.headers.Authorization = `Bearer ${token}`

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

    const { language, token } = sessionStore.getSnapshot()
    const apiError = toApiError(error, language)

    if (apiError.status === 401) {
      if (token) {
        sessionStore.signOut()
        notify('warning', translate(language, 'error.unauthorized'), 'unauthorized')
      }
    } else if (!(isAxiosError(error) && error.config?.skipErrorNotification)) {
      // Çevrimdışıyken sticky banner varken aynı metni toast ile tekrarlama.
      if (!(apiError.code === 'network_error' && !isOnline)) {
        notify('error', apiError.message, apiError.resourceKey ?? apiError.code)
      }
    }

    return Promise.reject(apiError)
  },
)
