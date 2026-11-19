import { queryOptions } from '@tanstack/react-query'
import { toApiError } from '@/core/api/apiError'
import { axiosClient } from '@/core/api/axiosClient'
import { sessionStore } from '@/core/session'
import type { ChangePasswordRequest, MfaSetupData, ActiveSessionItem } from '@/types/security'
import { queryKeys } from '@/services/queryKeys'

const skipNotify = { skipErrorNotification: true } as const

async function wrap<T>(run: () => Promise<T>): Promise<T> {
  try {
    return await run()
  } catch (error) {
    const { language } = sessionStore.getSnapshot()
    throw toApiError(error, language)
  }
}

/**
 * Self-servis güvenlik: oturumlar (Issue #35), şifre değiştirme (#30), MFA (#38).
 */
export const accountSecurityService = {
  async getSessions(signal?: AbortSignal): Promise<ActiveSessionItem[]> {
    return wrap(async () => {
      const { data } = await axiosClient.get<ActiveSessionItem[]>('/sessions', { signal })
      return data
    })
  },

  async revokeSession(sessionId: string): Promise<void> {
    await wrap(async () => {
      await axiosClient.delete(`/sessions/${sessionId}`, skipNotify)
    })
  },

  async revokeOtherSessions(): Promise<{ message: string }> {
    return wrap(async () => {
      const { data } = await axiosClient.post<{ message: string }>(
        '/sessions/revoke-others',
        {},
        skipNotify,
      )
      return data
    })
  },

  async changePassword(payload: ChangePasswordRequest): Promise<{ message: string }> {
    return wrap(async () => {
      const { data } = await axiosClient.post<{ message: string }>(
        '/auth/change-password',
        {
          currentPassword: payload.currentPassword,
          newPassword: payload.newPassword,
        },
        skipNotify,
      )
      return data
    })
  },

  async setupMfa(): Promise<MfaSetupData> {
    return wrap(async () => {
      const { data } = await axiosClient.post<MfaSetupData>('/auth/mfa/setup', {}, skipNotify)
      return data
    })
  },

  async verifyMfa(code: string): Promise<{ message: string }> {
    return wrap(async () => {
      const { data } = await axiosClient.post<{ message: string }>(
        '/auth/mfa/verify',
        { code: code.trim() },
        skipNotify,
      )
      return data
    })
  },

  async disableMfa(password: string): Promise<{ message: string }> {
    return wrap(async () => {
      const { data } = await axiosClient.post<{ message: string }>(
        '/auth/mfa/disable',
        { password },
        skipNotify,
      )
      return data
    })
  },
}

export const accountSecurityQueries = {
  sessions: () =>
    queryOptions({
      queryKey: [...queryKeys.sessions, 'list'],
      queryFn: ({ signal }) => accountSecurityService.getSessions(signal),
    }),
}
