import axios from 'axios'
import { ApiError, toApiError } from '../core/api/apiError'
import { axiosClient } from '../core/api/axiosClient'
import { refreshAccessToken } from '../core/api/refreshAccessToken'
import { sessionStore } from '../core/session'
import type { AuthTokens, LoginRequest, LoginResult } from '../types/auth'
import { sessionService } from './sessionService'

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || '/api/v1'

/**
 * Gerçek kimlik akışı: login → token çifti → `/auth/me`; logout sunucuya refresh iptali gönderir.
 * Refresh interceptor `refreshAccessToken` ile tek uçuş rotation yapar.
 */
export const authService = {
  async login(request: LoginRequest): Promise<LoginResult> {
    try {
      const { data } = await axiosClient.post<AuthTokens>(
        '/auth/login',
        {
          email: request.email.trim(),
          password: request.password,
          totpCode: request.totpCode?.trim() || undefined,
          recoveryCode: request.recoveryCode?.trim() || undefined,
        },
        { skipErrorNotification: true },
      )

      sessionStore.signIn({
        accessToken: data.accessToken,
        refreshToken: data.refreshToken,
        expiresAt: data.expiresAt,
      })

      const me = await sessionService.getCurrent()
      return {
        accessToken: data.accessToken,
        refreshToken: data.refreshToken,
        expiresAt: data.expiresAt,
        userName: me.email || `${me.firstName} ${me.lastName}`.trim() || me.username,
        tenant: sessionStore.getSnapshot().tenant,
        roles: me.roles,
      }
    } catch (error) {
      const { language } = sessionStore.getSnapshot()
      throw error instanceof ApiError ? error : toApiError(error, language)
    }
  },

  refresh: refreshAccessToken,

  async logout(): Promise<void> {
    const { refreshToken } = sessionStore.getSnapshot()
    try {
      if (refreshToken) {
        await axios.post(
          `${API_BASE_URL}/auth/logout`,
          { refreshToken },
          { headers: { Accept: 'application/json' }, timeout: 10_000 },
        )
      }
    } catch {
      // Sunucu hatası olsa bile istemci oturumu temizlenir.
    } finally {
      sessionStore.signOut()
    }
  },
}
