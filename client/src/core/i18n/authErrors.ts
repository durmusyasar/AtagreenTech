import { ApiError } from '../api/apiError'
import type { Language } from './languages'

/**
 * UserService `AUTH_*` / ağ hata kodları → kullanıcıya gösterilecek TR/EN metinler (Issue #36 / #41).
 * Backend kod adları kaynağı: `AuthErrorCodes` (UserService).
 */
const AUTH_ERROR_MESSAGES: Record<Language, Record<string, string>> = {
  tr: {
    AUTH_INVALID_CREDENTIALS: 'E-posta adresi veya şifre hatalı.',
    AUTH_ACCOUNT_LOCKED:
      'Çok fazla başarısız deneme nedeniyle hesabınız geçici olarak kilitlendi. Lütfen daha sonra tekrar deneyin.',
    AUTH_ACCOUNT_INACTIVE: 'Hesabınız pasif durumdadır. Lütfen sistem yöneticinizle iletişime geçin.',
    AUTH_EMAIL_NOT_VERIFIED: 'Giriş yapabilmek için lütfen e-posta adresinizi doğrulayın.',
    AUTH_RATE_LIMIT_EXCEEDED: 'Çok fazla deneme yaptınız. Güvenliğiniz için lütfen 1 dakika bekleyin.',
    AUTH_MFA_INVALID: 'Girdiğiniz doğrulama veya kurtarma kodu geçersiz.',
    /** Taslak / harici istemcilerle uyum için eşanlamlı kod. */
    MFA_INVALID_CODE: 'Girdiğiniz doğrulama veya kurtarma kodu geçersiz.',
    NETWORK_ERROR: 'Sunucuya erişilemiyor. İnternet bağlantınızı kontrol edin.',
    UNKNOWN_ERROR: 'Giriş sırasında beklenmeyen bir hata oluştu.',
  },
  en: {
    AUTH_INVALID_CREDENTIALS: 'Invalid email address or password.',
    AUTH_ACCOUNT_LOCKED:
      'Your account is temporarily locked due to multiple failed attempts. Please try again later.',
    AUTH_ACCOUNT_INACTIVE: 'Your account is inactive. Please contact your system administrator.',
    AUTH_EMAIL_NOT_VERIFIED: 'Please verify your email address before signing in.',
    AUTH_RATE_LIMIT_EXCEEDED: 'Too many requests. Please wait 1 minute before trying again.',
    AUTH_MFA_INVALID: 'The verification or recovery code you entered is invalid.',
    MFA_INVALID_CODE: 'The verification or recovery code you entered is invalid.',
    NETWORK_ERROR: 'Unable to reach the server. Please check your connection.',
    UNKNOWN_ERROR: 'An unexpected error occurred during sign in.',
  },
}

/** Bilinen auth hata kodlarından birine çevrilebilir mi? */
export function isKnownAuthErrorCode(code: string | null | undefined): boolean {
  if (!code) return false
  return Object.prototype.hasOwnProperty.call(AUTH_ERROR_MESSAGES.tr, code)
}

/**
 * Backend / ApiError kodunu dil seçimine göre kullanıcı dostu mesaja çevirir.
 * `AUTH_MFA_REQUIRED` bilinçli olarak sözlükte yok — UX ikinci adıma geçmeli, hata göstermemeli.
 */
export function resolveAuthErrorMessage(error: unknown, locale: Language = 'tr'): string {
  const dictionary = AUTH_ERROR_MESSAGES[locale] ?? AUTH_ERROR_MESSAGES.tr

  if (error instanceof ApiError) {
    if (error.code === 'network_error' || error.status === null) return dictionary.NETWORK_ERROR
    if (isKnownAuthErrorCode(error.code)) return dictionary[error.code]
    return dictionary.UNKNOWN_ERROR
  }

  if (typeof error === 'object' && error !== null && 'response' in error) {
    const response = (error as { response?: { data?: { code?: string; message?: string; errorCode?: string } } })
      .response
    if (!response) return dictionary.NETWORK_ERROR

    const rawCode = response.data?.code || response.data?.errorCode || ''
    const rawMessage = response.data?.message || ''
    const extractedCode = rawCode || rawMessage.split(':')[0]?.trim() || ''

    if (isKnownAuthErrorCode(extractedCode)) return dictionary[extractedCode]
  }

  if (typeof error === 'string' && isKnownAuthErrorCode(error)) {
    return dictionary[error]
  }

  return dictionary.UNKNOWN_ERROR
}
