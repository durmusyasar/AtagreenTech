import { toApiError } from '@/core/api/apiError'
import { axiosClient } from '@/core/api/axiosClient'
import { sessionStore } from '@/core/session'
import type { AuthTokens, LoginRequest, LoginResult } from '@/types/auth'
import type { CurrentSession } from '@/types/session'
import { sessionService } from '@/services/sessionService'

function displayName(me: CurrentSession): string {
  return me.email || `${me.firstName} ${me.lastName}`.trim() || me.username
}

/**
 * Gerçek kimlik akışı: login → token çifti → `/auth/me`; logout sunucuya refresh iptali gönderir.
 * Refresh interceptor `refreshAccessToken` ile tek uçuş rotation yapar (bu servisten re-export yok).
 */
export const authService = {
  async login(request: LoginRequest): Promise<LoginResult> {
    const rememberMe = request.rememberMe === true
    const { language } = sessionStore.getSnapshot()

    try {
      const { data } = await axiosClient.post<AuthTokens>(
        '/auth/login',
        {
          email: request.email.trim(),
          password: request.password,
          totpCode: request.totpCode?.trim() || undefined,
          recoveryCode: request.recoveryCode?.trim() || undefined,
          rememberMe,
        },
        { skipErrorNotification: true },
      )

      sessionStore.signIn({
        accessToken: data.accessToken,
        refreshToken: data.refreshToken,
        expiresAt: data.expiresAt,
        rememberMe,
      })

      try {
        const me = await sessionService.getCurrent()
        return {
          ...data,
          userName: displayName(me),
          tenant: sessionStore.getSnapshot().tenant,
          roles: me.roles,
        }
      } catch (error) {
        sessionStore.signOut()
        throw toApiError(error, language)
      }
    } catch (error) {
      throw toApiError(error, language)
    }
  },

  async logout(): Promise<void> {
    const { refreshToken } = sessionStore.getSnapshot()
    try {
      if (refreshToken) {
        await axiosClient.post(
          '/auth/logout',
          { refreshToken },
          { skipErrorNotification: true },
        )
      }
    } catch {
      // Sunucu hatası olsa bile istemci oturumu temizlenir.
    } finally {
      sessionStore.signOut()
    }
  },
}
