import { ApiError } from '@/core/api/apiError'
import { isKnownAuthErrorCode, resolveAuthErrorMessage } from '@/core/i18n/authErrors'
import type { Language } from '@/core/i18n/languages'

/**
 * Public auth formları (accept-invite / reset-password):
 * bilinen kod / network → i18n; AUTH_BAD_REQUEST vb. → sunucu mesajı (UNKNOWN_ERROR yutulmasın).
 */
export function resolveAuthFormError(error: unknown, language: Language, fallback: string): string {
  if (!(error instanceof ApiError)) return fallback

  if (error.code === 'network_error' || error.status === null || isKnownAuthErrorCode(error.code)) {
    return resolveAuthErrorMessage(error, language)
  }

  return error.message || fallback
}
