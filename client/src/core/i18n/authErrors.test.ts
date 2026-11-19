import { beforeAll, describe, expect, it } from 'vitest'
import '../../i18n'
import { ApiError } from '@/core/api/apiError'
import { isKnownAuthErrorCode, resolveAuthErrorMessage } from '@/core/i18n/authErrors'

beforeAll(async () => {
  const i18n = (await import('../../i18n')).default
  await i18n.init()
})

describe('resolveAuthErrorMessage', () => {
  it('maps AUTH_* codes for TR and EN', () => {
    const locked = new ApiError({
      message: 'locked',
      status: 423,
      code: 'AUTH_ACCOUNT_LOCKED',
    })
    expect(resolveAuthErrorMessage(locked, 'tr')).toContain('kilitlendi')
    expect(resolveAuthErrorMessage(locked, 'en')).toContain('temporarily locked')
  })

  it('maps AUTH_MFA_INVALID and MFA_INVALID_CODE alias', () => {
    expect(
      resolveAuthErrorMessage(new ApiError({ message: 'x', status: 401, code: 'AUTH_MFA_INVALID' }), 'tr'),
    ).toContain('geçersiz')
    expect(resolveAuthErrorMessage('MFA_INVALID_CODE', 'en')).toContain('invalid')
  })

  it('returns network message when there is no response', () => {
    expect(resolveAuthErrorMessage({ response: undefined }, 'tr')).toContain('erişilemiyor')
    expect(
      resolveAuthErrorMessage(new ApiError({ message: 'n', status: null, code: 'network_error' }), 'en'),
    ).toContain('Unable to reach')
  })

  it('extracts code from message prefix when code field is empty', () => {
    expect(
      resolveAuthErrorMessage(
        { response: { data: { message: 'AUTH_INVALID_CREDENTIALS: bad password' } } },
        'en',
      ),
    ).toContain('Invalid email')
  })

  it('falls back to unknown for unrecognized codes', () => {
    expect(
      resolveAuthErrorMessage(new ApiError({ message: 'x', status: 500, code: 'SOMETHING_ELSE' }), 'tr'),
    ).toContain('beklenmeyen')
  })
})

describe('isKnownAuthErrorCode', () => {
  it('recognizes AUTH_INVALID_CREDENTIALS', () => {
    expect(isKnownAuthErrorCode('AUTH_INVALID_CREDENTIALS')).toBe(true)
    expect(isKnownAuthErrorCode('AUTH_MFA_REQUIRED')).toBe(false)
  })

  it('recognizes AUTH_EMAIL_NOT_FOUND', () => {
    expect(isKnownAuthErrorCode('AUTH_EMAIL_NOT_FOUND')).toBe(true)
    expect(
      resolveAuthErrorMessage(
        new ApiError({ message: 'x', status: 400, code: 'AUTH_EMAIL_NOT_FOUND' }),
        'tr',
      ),
    ).toContain('kayıtlı değil')
  })
})
