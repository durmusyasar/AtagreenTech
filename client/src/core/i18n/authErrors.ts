import type { Language } from './languages'
import { isKnownAuthErrorCode, resolveApiError } from '../../i18n/resolveApiError'

export { isKnownAuthErrorCode, resolveApiError }

/**
 * Backend / ApiError kodunu dil seçimine göre kullanıcı dostu mesaja çevirir.
 * Sözlük `auth:errors.*` (react-i18next); `AUTH_MFA_REQUIRED` kasıtlı olarak yok.
 */
export function resolveAuthErrorMessage(error: unknown, locale: Language = 'tr'): string {
  return resolveApiError(error, locale)
}
