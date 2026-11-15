import { ApiError } from '../core/api/apiError'
import { axiosClient } from '../core/api/axiosClient'
import { isExpired, readTokenClaims } from '../core/jwt'
import { sessionStore } from '../core/session'
import type { LoginRequest, LoginResult } from '../types/auth'
import type { CurrentSession } from '../types/session'

/**
 * Erişim token'ını yerel olarak kontrol eder, API'nin kabul ettiğini
 * `GET /security-test/protected` ile doğrular ve oturuma yazar.
 * Kimlik servisi gelince bu akış `/auth/login` ile değişir.
 *
 * Token önce oturuma yazılmaz; böylece geçersiz denemelerde interceptor 401 ile global uyarı göstermez.
 */
export const authService = {
  async login({ accessToken }: LoginRequest): Promise<LoginResult> {
    const token = accessToken.trim()
    const claims = readTokenClaims(token)
    if (!claims) {
      throw new ApiError({ message: 'invalid_token', status: 400, code: 'invalid_token' })
    }
    if (isExpired(claims)) {
      throw new ApiError({ message: 'token_expired', status: 401, code: 'token_expired' })
    }

    const { data } = await axiosClient.get<CurrentSession>('/security-test/protected', {
      headers: { Authorization: `Bearer ${token}` },
      skipErrorNotification: true,
    })
    sessionStore.signIn(token)
    return toLoginResult(token, data, claims.email ?? claims.subject)
  },
}

function toLoginResult(token: string, session: CurrentSession, fallbackName: string | null): LoginResult {
  return {
    token,
    userName: session.email ?? session.userId ?? fallbackName ?? 'user',
    tenant: session.tenant ?? sessionStore.getSnapshot().tenant,
    roles: session.roles,
  }
}
