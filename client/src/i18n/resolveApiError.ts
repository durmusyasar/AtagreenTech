import { ApiError } from '@/core/api/apiError'
import type { Language } from '@/core/i18n/languages'
import i18n from '@/i18n/index'

const KNOWN_AUTH_ERROR_CODES = new Set([
  'AUTH_INVALID_CREDENTIALS',
  'AUTH_ACCOUNT_LOCKED',
  'AUTH_ACCOUNT_INACTIVE',
  'AUTH_EMAIL_NOT_VERIFIED',
  'AUTH_EMAIL_NOT_FOUND',
  'AUTH_RATE_LIMIT_EXCEEDED',
  'AUTH_MFA_INVALID',
  'MFA_INVALID_CODE',
  'NETWORK_ERROR',
  'UNKNOWN_ERROR',
])

/** Bilinen auth hata kodlarından biri mi? (`AUTH_MFA_REQUIRED` kasıtlı olarak yok.) */
export function isKnownAuthErrorCode(code: string | null | undefined): boolean {
  if (!code) return false
  return KNOWN_AUTH_ERROR_CODES.has(code)
}

function authError(locale: Language | undefined, code: string): string {
  const t = i18n.getFixedT(locale ?? i18n.resolvedLanguage ?? 'tr', 'enum')
  return t(`errors.${code}` as 'errors.UNKNOWN_ERROR')
}

/**
 * Backend / ApiError mesajını tercih eder; yoksa `enum:errors.<CODE>` fallback (#71).
 */
export function resolveApiError(error: unknown, locale?: Language): string {
  if (error instanceof ApiError) {
    if (error.code === 'network_error' || error.status === null) return authError(locale, 'NETWORK_ERROR')
    if (error.message?.trim()) return error.message
    if (isKnownAuthErrorCode(error.code)) return authError(locale, error.code)
    return authError(locale, 'UNKNOWN_ERROR')
  }

  if (typeof error === 'object' && error !== null && 'response' in error) {
    const response = (error as {
      response?: { data?: { code?: string; message?: string; errorCode?: string; detail?: string } }
    }).response
    if (!response) return authError(locale, 'NETWORK_ERROR')

    const backendMessage = response.data?.detail || response.data?.message || ''
    if (backendMessage.trim()) return backendMessage

    const rawCode = response.data?.code || response.data?.errorCode || ''
    const extractedCode = rawCode || ''

    if (extractedCode && isKnownAuthErrorCode(extractedCode)) return authError(locale, extractedCode)
  }

  if (typeof error === 'string') {
    if (isKnownAuthErrorCode(error)) return authError(locale, error)
    if (error.trim()) return error
  }

  return authError(locale, 'UNKNOWN_ERROR')
}
