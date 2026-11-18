import type { MfaMode } from '@/routes/auth/login/type'

export interface MfaVerificationStepProps {
  mode: MfaMode
  onToggleMode: () => void
  onBack: () => void
  /** Submit sırasında geri / mod değişimini kilitle. */
  disabled?: boolean
}
