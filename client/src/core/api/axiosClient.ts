import axios, { isAxiosError, isCancel } from 'axios'
import { translate } from '../i18n/messages'
import { sessionStore } from '../session'
import { toApiError } from './apiError'
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
 */
export const axiosClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || '/api/v1',
  timeout: 30_000,
  headers: { Accept: 'application/json' },
})

axiosClient.interceptors.request.use((config) => {
  const { token, tenant, isTenantFromHost, language } = sessionStore.getSnapshot()

  if (token) config.headers.Authorization = `Bearer ${token}`

  // Alt alan adı olan adreslerde tenant'ı API host'tan çözer; başlık yalnızca geliştirmede (localhost) gerekir.
  if (tenant && !isTenantFromHost) config.headers['X-Tenant'] = tenant

  config.headers['Accept-Language'] = language
  return config
})

axiosClient.interceptors.response.use(
  (response) => response,
  (error: unknown) => {
    if (isCancel(error)) return Promise.reject(error)

    const { language, token } = sessionStore.getSnapshot()
    const apiError = toApiError(error, language)

    if (apiError.status === 401) {
      if (token) {
        sessionStore.signOut()
        notify('warning', translate(language, 'error.unauthorized'), 'unauthorized')
      }
    } else if (!(isAxiosError(error) && error.config?.skipErrorNotification)) {
      notify('error', apiError.message, apiError.resourceKey ?? apiError.code)
    }

    return Promise.reject(apiError)
  },
)
