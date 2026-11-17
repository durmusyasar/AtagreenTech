import { queryOptions } from '@tanstack/react-query'
import { axiosClient } from '@/core/api/axiosClient'
import type { CurrentSession } from '@/types/session'
import { queryKeys } from '@/services/queryKeys'

export const sessionService = {
  /** Giriş yapmış kullanıcının profili (`GET /auth/me`). */
  async getCurrent(signal?: AbortSignal): Promise<CurrentSession> {
    const { data } = await axiosClient.get<CurrentSession>('/auth/me', { signal })
    return data
  },
}

export const sessionQueries = {
  current: () =>
    queryOptions({
      queryKey: [...queryKeys.session, 'current'],
      queryFn: ({ signal }) => sessionService.getCurrent(signal),
    }),
}
