/** @vitest-environment happy-dom */
import { describe, expect, it } from 'vitest'
import { createLoginSchema } from '@/routes/auth/login/validation/loginSchema'

const base = {
  email: '',
  password: '',
  rememberMe: false,
  mfaInput: '',
}

describe('createLoginSchema', () => {
  it('rejects empty email/password with i18n keys', () => {
    const result = createLoginSchema('CREDENTIALS', 'TOTP').safeParse(base)
    expect(result.success).toBe(false)
    if (result.success) return
    const messages = result.error.issues.map((issue) => issue.message)
    expect(messages).toContain('validation.emailRequired')
    expect(messages).toContain('validation.passwordRequired')
  })

  it('rejects invalid email', () => {
    const result = createLoginSchema('CREDENTIALS', 'TOTP').safeParse({
      ...base,
      email: 'not-an-email',
      password: 'secret1',
    })
    expect(result.success).toBe(false)
    if (result.success) return
    expect(result.error.issues.some((issue) => issue.message === 'validation.emailInvalid')).toBe(
      true,
    )
  })

  it('rejects bare local-part email like "asd"', () => {
    const result = createLoginSchema('CREDENTIALS', 'TOTP').safeParse({
      ...base,
      email: 'asd',
      password: 'secret1',
    })
    expect(result.success).toBe(false)
    if (result.success) return
    expect(result.error.issues.some((issue) => issue.path[0] === 'email')).toBe(true)
    expect(result.error.issues.some((issue) => issue.message === 'validation.emailInvalid')).toBe(
      true,
    )
  })

  it('validates TOTP with superRefine on MFA step', () => {
    const invalid = createLoginSchema('MFA', 'TOTP').safeParse({
      ...base,
      email: 'admin@omnisync.com',
      password: 'secret1',
      mfaInput: '12ab',
    })
    expect(invalid.success).toBe(false)

    const valid = createLoginSchema('MFA', 'TOTP').safeParse({
      ...base,
      email: 'admin@omnisync.com',
      password: 'secret1',
      mfaInput: '123456',
    })
    expect(valid.success).toBe(true)
  })
})
