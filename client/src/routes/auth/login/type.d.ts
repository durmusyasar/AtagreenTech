export type LoginStep = 'CREDENTIALS' | 'MFA'
export type MfaMode = 'TOTP' | 'RECOVERY'
export type MfaInputMode = MfaMode

export interface LoginForm {
  email: string
  password: string
  mfaInput?: string
}

export interface MfaVerificationStepProps {
  mode: MfaInputMode
  onToggleMode: () => void
  onBack: () => void
}
