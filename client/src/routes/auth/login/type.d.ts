export type LoginStep = 'CREDENTIALS' | 'MFA'
export type MfaMode = 'TOTP' | 'RECOVERY'

/** RHF + Zod ortak form değerleri (`name` / schema key birebir). */
export interface LoginFormValues {
  email: string
  password: string
  rememberMe: boolean
  mfaInput: string
}

/** @deprecated LoginFormValues kullanın. */
export type LoginForm = LoginFormValues
