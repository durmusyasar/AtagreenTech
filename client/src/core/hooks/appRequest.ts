import type { AxiosRequestConfig } from 'axios'
import { axiosClient } from '../api/axiosClient'

/** GET yanıt gövdesini döndüren TanStack `queryFn`. `signal` iptali axios'a iletilir. */
export function createGetQueryFn<TData>(
  url: string,
  params?: object,
  config?: AxiosRequestConfig,
): (context: { signal: AbortSignal }) => Promise<TData> {
  return async ({ signal }) => {
    const { data } = await axiosClient.get<TData>(url, { ...config, params, signal })
    return data
  }
}

export type AppMutationMethod = 'post' | 'put' | 'patch' | 'delete'

/**
 * POST / PUT / PATCH / DELETE çağrısı. `body` verilmezse değişkenler gövde olur; DELETE'te gövde
 * yalnızca `body` açıkça verildiğinde gönderilir (çoğu silme yalnızca yol kullanır).
 */
export async function executeMutationRequest<TData, TVariables>(options: {
  url: string | ((variables: TVariables) => string)
  method?: AppMutationMethod
  variables: TVariables
  body?: (variables: TVariables) => unknown
  skipErrorNotification?: boolean
}): Promise<TData> {
  const { variables, body, skipErrorNotification } = options
  const method = options.method ?? 'post'
  const path = typeof options.url === 'function' ? options.url(variables) : options.url
  const config: AxiosRequestConfig = { skipErrorNotification }
  const payload = body ? body(variables) : variables

  if (method === 'delete') {
    const { data } = await axiosClient.delete<TData>(path, body ? { ...config, data: payload } : config)
    return data
  }

  if (method === 'put') {
    const { data } = await axiosClient.put<TData>(path, payload, config)
    return data
  }

  if (method === 'patch') {
    const { data } = await axiosClient.patch<TData>(path, payload, config)
    return data
  }

  const { data } = await axiosClient.post<TData>(path, payload, config)
  return data
}
