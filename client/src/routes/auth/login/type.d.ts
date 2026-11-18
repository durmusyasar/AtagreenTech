export type LoginStep = 'CREDENTIALS' | 'MFA'
export type MfaMode = 'TOTP' | 'RECOVERY'
export type MfaInputMode = MfaMode

export interface LoginForm {
  email: string
  password: string
  rememberMe?: boolean
  mfaInput?: string
}

export interface LoginFieldErrors {
  email?: string
  password?: string
  mfaInput?: string
}

export interface MfaVerificationStepProps {
  mode: MfaInputMode
  value: string
  onChange: (value: string) => void
  onToggleMode: () => void
  onBack: () => void
  /** Zod alan hatası (dil değişiminde güncellenir). */
  error?: string
}
