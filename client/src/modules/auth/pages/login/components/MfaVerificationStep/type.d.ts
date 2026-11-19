import type { MfaMode } from '@modules/auth/pages/login/type'

export interface MfaVerificationStepProps {
  mode: MfaMode
  onToggleMode: () => void
  onBack: () => void
  /** Submit sırasında geri / mod değişimini kilitle. */
  disabled?: boolean
}
