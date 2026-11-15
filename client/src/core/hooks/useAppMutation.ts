import { useMutation, type UseMutationOptions, type UseMutationResult } from '@tanstack/react-query'
import type { ApiError } from '../api/apiError'
import { executeMutationRequest, type AppMutationMethod } from './appRequest'

export type UseAppMutationOptions<TData, TVariables> = Omit<
  UseMutationOptions<TData, ApiError, TVariables>,
  'mutationFn'
> & {
  /**
   * Özel mutasyon (ör. oturum kurma). Verilirse `url` / `method` kullanılmaz.
   * Kimlik servisi gelene kadar login gibi axios dışı adımlar buradan geçer.
   */
  mutationFn?: (variables: TVariables) => Promise<TData>
  url?: string | ((variables: TVariables) => string)
  method?: AppMutationMethod
  /**
   * Form alanlarında gösterilecek hatalarda global bildirimi kapatır (`axiosClient` interceptor).
   * 401 yine oturumu kapatır. Yalnızca `url` akışında axios config'e yazılır.
   */
  skipErrorNotification?: boolean
  /** Varsayılan: değişkenler gövde olarak gider. DELETE'te gövde yalnızca bu verilirse gönderilir. */
  body?: (variables: TVariables) => unknown
}

/**
 * POST / PUT / PATCH / DELETE mutasyonu veya özel `mutationFn`. Hatalar {@link ApiError}; formlar
 * `skipErrorNotification: true` ile alanı kendisi boyar.
 */
export function useAppMutation<TData = unknown, TVariables = void>(
  options: UseAppMutationOptions<TData, TVariables>,
): UseMutationResult<TData, ApiError, TVariables> {
  const { url, method, skipErrorNotification, body, mutationFn, ...rest } = options

  return useMutation({
    mutationFn: (variables) => {
      if (mutationFn) return mutationFn(variables)
      if (url === undefined) {
        return Promise.reject(new Error('useAppMutation requires url or mutationFn'))
      }
      return executeMutationRequest<TData, TVariables>({
        url,
        method,
        variables,
        body,
        skipErrorNotification,
      })
    },
    ...rest,
  })
}
