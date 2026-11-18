import type { LoginFormValues, LoginStep, MfaMode } from '@/routes/auth/login/type'

/** Auth sonrası varsayılan yönlendirme (state.from yoksa). */
export const LOGIN_DEFAULT_REDIRECT = '/' as const

export const LOGIN_STEP = {
  Credentials: 'CREDENTIALS',
  Mfa: 'MFA',
} as const satisfies Record<string, LoginStep>

export const MFA_MODE = {
  Totp: 'TOTP',
  Recovery: 'RECOVERY',
} as const satisfies Record<string, MfaMode>

/** RHF `defaultValues` — alan adları schema ile birebir. */
export const LOGIN_FORM_DEFAULTS: LoginFormValues = {
  email: '',
  password: '',
  rememberMe: false,
  mfaInput: '',
}

/** Query string başarı bayrakları (`?invited=1`). */
export const LOGIN_QUERY = {
  invited: 'invited',
  reset: 'reset',
  passwordChanged: 'passwordChanged',
} as const

export const LOGIN_QUERY_FLAG = '1' as const

export const LOGIN_PASSWORD_MIN_LENGTH = 6

export const TOTP_CODE_LENGTH = 6
export const RECOVERY_CODE_LENGTH = 8

/** MFA TOTP: tam 6 rakam. */
export const TOTP_CODE_PATTERN = new RegExp(`^\\d{${TOTP_CODE_LENGTH}}$`)

/** MFA recovery: 8 alfanümerik karakter. */
export const RECOVERY_CODE_PATTERN = new RegExp(`^[A-Za-z0-9]{${RECOVERY_CODE_LENGTH}}$`)
