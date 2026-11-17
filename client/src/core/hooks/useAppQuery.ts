import {
  useQuery,
  type QueryKey,
  type UseQueryOptions,
  type UseQueryResult,
} from '@tanstack/react-query'
import type { AxiosRequestConfig } from 'axios'
import type { ApiError } from '@/core/api/apiError'
import { createGetQueryFn } from '@/core/hooks/appRequest'

/**
 * URL + queryKey ile GET sorgusu. Axios istemcisi tenant / dil / yetki başlıklarını ekler; hatalar
 * {@link ApiError} olarak gelir (`queryClient` Register.defaultError).
 *
 * Servis katmanındaki `queryOptions(...)` fabrikaları da aynı imzayla geçirilebilir; böylece paylaşılan
 * anahtarlar (ör. ürün listesi) tek yerde kalır.
 */
export type UseAppQueryUrlOptions<TData, TSelected = TData, TParams extends object = Record<string, unknown>> = Omit<
  UseQueryOptions<TData, ApiError, TSelected, QueryKey>,
  'queryKey' | 'queryFn'
> & {
  url: string
  queryKey: QueryKey
  /** Axios `params` (sayfalama, arama, dil…). */
  params?: TParams
  config?: AxiosRequestConfig
}

export function useAppQuery<TData, TSelected = TData, TParams extends object = Record<string, unknown>>(
  options: UseAppQueryUrlOptions<TData, TSelected, TParams>,
): UseQueryResult<TSelected, ApiError>

export function useAppQuery<TData, TSelected = TData, TQueryKey extends QueryKey = QueryKey>(
  options: UseQueryOptions<TData, ApiError, TSelected, TQueryKey>,
): UseQueryResult<TSelected, ApiError>

export function useAppQuery<TData, TSelected = TData>(
  options: UseAppQueryUrlOptions<TData, TSelected> | UseQueryOptions<TData, ApiError, TSelected>,
): UseQueryResult<TSelected, ApiError> {
  // Hook sırası sabit kalsın diye seçenekler önce normalize edilir; `useQuery` tek kez çağrılır.
  const normalized: UseQueryOptions<TData, ApiError, TSelected> = isUrlOptions(options)
    ? {
        ...omitUrlFields(options),
        queryKey: options.queryKey,
        queryFn: createGetQueryFn<TData>(options.url, options.params, options.config),
      }
    : options

  return useQuery(normalized)
}

function isUrlOptions<TData, TSelected>(
  options: UseAppQueryUrlOptions<TData, TSelected> | UseQueryOptions<TData, ApiError, TSelected>,
): options is UseAppQueryUrlOptions<TData, TSelected> {
  return 'url' in options && typeof options.url === 'string'
}

function omitUrlFields<TData, TSelected>(
  options: UseAppQueryUrlOptions<TData, TSelected>,
): Omit<UseAppQueryUrlOptions<TData, TSelected>, 'url' | 'queryKey' | 'params' | 'config'> {
  const { url: _url, queryKey: _queryKey, params: _params, config: _config, ...rest } = options
  return rest
}
