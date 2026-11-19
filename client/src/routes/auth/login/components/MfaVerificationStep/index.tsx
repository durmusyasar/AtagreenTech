import { FormFloatingInput } from '@/Components/forms/FormFloatingInput'
import { useLanguage } from '@/core/context/useLanguage'
import {
  MFA_MODE,
  RECOVERY_CODE_LENGTH,
  TOTP_CODE_LENGTH,
} from '@/routes/auth/login/constants'
import type { LoginFormValues } from '@/routes/auth/login/type'
import type { MfaVerificationStepProps } from '@/routes/auth/login/components/MfaVerificationStep/type'

/** Login MFA ikinci adım — `name="mfaInput"` FormProvider üzerinden Zod’a bağlı. */
export function MfaVerificationStep({
  mode,
  onToggleMode,
  onBack,
  disabled = false,
}: MfaVerificationStepProps) {
  const { t } = useLanguage()
  const isTotp = mode === MFA_MODE.Totp

  return (
    <div className="space-y-3">
      <FormFloatingInput<LoginFormValues>
        name="mfaInput"
        autoFocus
        autoComplete="one-time-code"
        maxLength={isTotp ? TOTP_CODE_LENGTH : RECOVERY_CODE_LENGTH}
        inputMode={isTotp ? 'numeric' : 'text'}
        label={isTotp ? t('login.mfaTotpLabel') : t('login.mfaRecoveryLabel')}
        autoClear
        disabled={disabled}
        transformValue={(value) =>
          isTotp ? value.replace(/\D/g, '') : value.toUpperCase()
        }
        className="[&_input]:text-center [&_input]:font-mono [&_input]:tracking-[0.35em]"
      />

      <div className="flex items-center justify-between gap-2">
        <button type="button" onClick={onBack} disabled={disabled} className="auth-text-link">
          ← {t('login.backToCredentials')}
        </button>
        <button
          type="button"
          onClick={onToggleMode}
          disabled={disabled}
          className="auth-text-link auth-text-link--brand"
        >
          {isTotp ? t('login.useRecoveryCode') : t('login.useTotpCode')}
        </button>
      </div>
    </div>
  )
}
