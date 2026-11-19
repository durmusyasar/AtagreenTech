import { describe, expect, it } from 'vitest'
import { buildLoginRequest } from '@modules/auth/pages/login/buildLoginRequest'
import { LOGIN_STEP, MFA_MODE } from '@modules/auth/pages/login/constants'

const values = {
  email: '  admin@omnisync.com  ',
  password: 'Secret1!',
  rememberMe: true,
  mfaInput: ' 123456 ',
}

describe('buildLoginRequest', () => {
  it('maps credentials without MFA fields', () => {
    expect(buildLoginRequest(values, LOGIN_STEP.Credentials, MFA_MODE.Totp)).toEqual({
      email: 'admin@omnisync.com',
      password: 'Secret1!',
      rememberMe: true,
    })
  })

  it('adds totpCode on MFA + TOTP', () => {
    expect(buildLoginRequest(values, LOGIN_STEP.Mfa, MFA_MODE.Totp)).toEqual({
      email: 'admin@omnisync.com',
      password: 'Secret1!',
      rememberMe: true,
      totpCode: '123456',
    })
  })

  it('uppercases recovery codes', () => {
    expect(
      buildLoginRequest(
        { ...values, mfaInput: 'abcd1234' },
        LOGIN_STEP.Mfa,
        MFA_MODE.Recovery,
      ),
    ).toEqual({
      email: 'admin@omnisync.com',
      password: 'Secret1!',
      rememberMe: true,
      recoveryCode: 'ABCD1234',
    })
  })
})
