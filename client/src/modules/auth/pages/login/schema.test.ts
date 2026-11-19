/** @vitest-environment happy-dom */
import { describe, expect, it } from 'vitest'
import {
  LOGIN_PASSWORD_MIN_LENGTH,
  LOGIN_STEP,
  LOGIN_VALIDATION,
  MFA_MODE,
} from '@modules/auth/pages/login/constants'
import { defaultValues } from '@modules/auth/pages/login/defaultValues'
import { createLoginSchema } from '@modules/auth/pages/login/schema'

const validCredentials = {
  ...defaultValues,
  email: 'admin@omnisync.com',
  password: 'secret1',
}

describe('createLoginSchema', () => {
  it('rejects empty email/password with i18n keys', () => {
    const result = createLoginSchema(LOGIN_STEP.Credentials, MFA_MODE.Totp).safeParse(defaultValues)
    expect(result.success).toBe(false)
    if (result.success) return
    const messages = result.error.issues.map((issue) => issue.message)
    expect(messages).toContain(LOGIN_VALIDATION.emailRequired)
    expect(messages).toContain(LOGIN_VALIDATION.passwordRequired)
  })

  it('rejects invalid email', () => {
    const result = createLoginSchema(LOGIN_STEP.Credentials, MFA_MODE.Totp).safeParse({
      ...defaultValues,
      email: 'not-an-email',
      password: 'secret1',
    })
    expect(result.success).toBe(false)
    if (result.success) return
    expect(
      result.error.issues.some((issue) => issue.message === LOGIN_VALIDATION.emailInvalid),
    ).toBe(true)
  })

  it('rejects bare local-part email like "asd"', () => {
    const result = createLoginSchema(LOGIN_STEP.Credentials, MFA_MODE.Totp).safeParse({
      ...defaultValues,
      email: 'asd',
      password: 'secret1',
    })
    expect(result.success).toBe(false)
    if (result.success) return
    expect(result.error.issues.some((issue) => issue.path[0] === 'email')).toBe(true)
    expect(
      result.error.issues.some((issue) => issue.message === LOGIN_VALIDATION.emailInvalid),
    ).toBe(true)
  })

  it('rejects password shorter than minimum length', () => {
    const result = createLoginSchema(LOGIN_STEP.Credentials, MFA_MODE.Totp).safeParse({
      ...defaultValues,
      email: 'admin@omnisync.com',
      password: 'a'.repeat(LOGIN_PASSWORD_MIN_LENGTH - 1),
    })
    expect(result.success).toBe(false)
    if (result.success) return
    expect(
      result.error.issues.some((issue) => issue.message === LOGIN_VALIDATION.passwordMinLength),
    ).toBe(true)
  })

  it('allows empty mfaInput on credentials step', () => {
    const result = createLoginSchema(LOGIN_STEP.Credentials, MFA_MODE.Totp).safeParse(
      validCredentials,
    )
    expect(result.success).toBe(true)
  })

  it('validates TOTP on MFA step', () => {
    const invalid = createLoginSchema(LOGIN_STEP.Mfa, MFA_MODE.Totp).safeParse({
      ...validCredentials,
      mfaInput: '12ab',
    })
    expect(invalid.success).toBe(false)
    if (!invalid.success) {
      expect(
        invalid.error.issues.some((issue) => issue.message === LOGIN_VALIDATION.totpInvalid),
      ).toBe(true)
    }

    const empty = createLoginSchema(LOGIN_STEP.Mfa, MFA_MODE.Totp).safeParse({
      ...validCredentials,
      mfaInput: '',
    })
    expect(empty.success).toBe(false)

    const valid = createLoginSchema(LOGIN_STEP.Mfa, MFA_MODE.Totp).safeParse({
      ...validCredentials,
      mfaInput: '123456',
    })
    expect(valid.success).toBe(true)
  })

  it('validates recovery code on MFA step', () => {
    const invalid = createLoginSchema(LOGIN_STEP.Mfa, MFA_MODE.Recovery).safeParse({
      ...validCredentials,
      mfaInput: 'short',
    })
    expect(invalid.success).toBe(false)
    if (!invalid.success) {
      expect(
        invalid.error.issues.some((issue) => issue.message === LOGIN_VALIDATION.recoveryInvalid),
      ).toBe(true)
    }

    const valid = createLoginSchema(LOGIN_STEP.Mfa, MFA_MODE.Recovery).safeParse({
      ...validCredentials,
      mfaInput: 'AbCd1234',
    })
    expect(valid.success).toBe(true)
  })
})
