import {
  useQueryClient,
  type FetchQueryOptions,
  type QueryClient,
  type QueryKey,
} from '@tanstack/react-query'
import type { AxiosRequestConfig } from 'axios'
import type { ApiError } from '../api/apiError'
import { createGetQueryFn } from './appRequest'

export type UseAppLazyQueryOptions<TData, TVariables extends object = Record<string, unknown>> = Omit<
  FetchQueryOptions<TData, ApiError, TData, QueryKey>,
  'queryKey' | 'queryFn'
> & {
  url: string | ((variables: TVariables) => string)
  queryKey: QueryKey | ((variables: TVariables) => QueryKey)
  /**
   * Axios sorgu parametreleri. Yol değişkenlerini (`id`) parametreye yazmamak için fonksiyon verilebilir;
   * nesne verilirse tetikleyici değişkenleriyle birleştirilir.
   */
  params?: Partial<TVariables> | ((variables: TVariables) => object)
  config?: AxiosRequestConfig
}

/** `queryClient.fetchQuery` ile tek seferlik / tetiklenebilir GET. Hook yalnızca istemciyi bağlar. */
export async function fetchAppQuery<TData, TVariables extends object>(
  queryClient: QueryClient,
  options: UseAppLazyQueryOptions<TData, TVariables>,
  variables?: TVariables,
): Promise<TData> {
  const defaults = typeof options.params === 'function' ? undefined : options.params
  const merged = { ...defaults, ...variables } as TVariables
  const path = typeof options.url === 'function' ? options.url(merged) : options.url
  const queryKey = typeof options.queryKey === 'function' ? options.queryKey(merged) : options.queryKey
  const requestParams = typeof options.params === 'function' ? options.params(merged) : merged
  const { url: _url, queryKey: _key, params: _params, config, ...rest } = options

  return queryClient.fetchQuery({
    queryKey,
    queryFn: createGetQueryFn<TData>(path, requestParams, config),
    ...rest,
  })
}

/**
 * İsteğe bağlı GET. Dönen tetikleyici önbelleğe yazar; aynı anahtarla sonraki `useAppQuery` önbellekten okur.
 *
 * @example
 * const [loadProduct] = useAppLazyQuery({
 *   url: ({ id }) => `/products/${id}`,
 *   queryKey: ({ id, language }) => [...queryKeys.products, 'detail', id, language],
 * })
 * await loadProduct({ id, language })
 */
export function useAppLazyQuery<TData, TVariables extends object = Record<string, unknown>>(
  options: UseAppLazyQueryOptions<TData, TVariables>,
) {
  const queryClient = useQueryClient()

  const trigger = (variables?: TVariables): Promise<TData> => fetchAppQuery(queryClient, options, variables)

  return [trigger] as const
}
