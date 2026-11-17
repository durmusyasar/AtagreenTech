export type LoginStep = 'CREDENTIALS' | 'MFA'
export type MfaMode = 'TOTP' | 'RECOVERY'

export interface LoginForm {
  email: string
  password: string
  mfaInput?: string
}
