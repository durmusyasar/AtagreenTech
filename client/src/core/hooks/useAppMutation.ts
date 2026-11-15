import { useMutation, type UseMutationOptions, type UseMutationResult } from '@tanstack/react-query'
import type { ApiError } from '../api/apiError'
import { executeMutationRequest, type AppMutationMethod } from './appRequest'

export type UseAppMutationOptions<TData, TVariables> = Omit<
  UseMutationOptions<TData, ApiError, TVariables>,
  'mutationFn'
> & {
  url: string | ((variables: TVariables) => string)
  method?: AppMutationMethod
  /**
   * Form alanlarında gösterilecek hatalarda global bildirimi kapatır (`axiosClient` interceptor).
   * 401 yine oturumu kapatır.
   */
  skipErrorNotification?: boolean
  /** Varsayılan: değişkenler gövde olarak gider. DELETE'te gövde yalnızca bu verilirse gönderilir. */
  body?: (variables: TVariables) => unknown
}

/**
 * POST / PUT / PATCH / DELETE mutasyonu. Hatalar {@link ApiError}; formlar `skipErrorNotification: true`
 * ile alanı kendisi boyar.
 */
export function useAppMutation<TData = unknown, TVariables = void>(
  options: UseAppMutationOptions<TData, TVariables>,
): UseMutationResult<TData, ApiError, TVariables> {
  const { url, method, skipErrorNotification, body, ...rest } = options

  return useMutation({
    mutationFn: (variables) =>
      executeMutationRequest<TData, TVariables>({
        url,
        method,
        variables,
        body,
        skipErrorNotification,
      }),
    ...rest,
  })
}
