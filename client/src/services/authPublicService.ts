import { ApiError, toApiError } from '../core/api/apiError'
import { axiosClient } from '../core/api/axiosClient'
import { sessionStore } from '../core/session'

export interface AcceptInvitePayload {
  token: string
  password: string
  firstName: string
  lastName: string
}

export interface ResetPasswordPayload {
  token: string
  newPassword: string
}

export interface AuthMessageResult {
  message: string
}

const skipNotify = { skipErrorNotification: true } as const

async function postAuthMessage(url: string, body: unknown): Promise<AuthMessageResult> {
  try {
    const { data } = await axiosClient.post<AuthMessageResult>(url, body, skipNotify)
    return data
  } catch (error) {
    const { language } = sessionStore.getSnapshot()
    throw error instanceof ApiError ? error : toApiError(error, language)
  }
}

/**
 * Oturumsuz kimlik uçları: davet kabul, e-posta doğrulama, şifre unut/sıfırla.
 */
export const authPublicService = {
  acceptInvite: (payload: AcceptInvitePayload) =>
    postAuthMessage('/auth/accept-invite', {
      token: payload.token,
      password: payload.password,
      firstName: payload.firstName.trim(),
      lastName: payload.lastName.trim(),
    }),

  verifyEmail: (token: string) => postAuthMessage('/auth/verify-email', { token }),

  resendVerification: (email: string) =>
    postAuthMessage('/auth/resend-verification', { email: email.trim() }),

  forgotPassword: (email: string) => postAuthMessage('/auth/forgot-password', { email: email.trim() }),

  resetPassword: (payload: ResetPasswordPayload) =>
    postAuthMessage('/auth/reset-password', {
      token: payload.token,
      newPassword: payload.newPassword,
    }),
}
