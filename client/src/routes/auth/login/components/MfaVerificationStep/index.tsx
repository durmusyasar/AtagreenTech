import { FormFloatingInput } from '@/Components/forms/FormFloatingInput'
import { useLanguage } from '@/core/context/useLanguage'
import type { LoginFormValues, MfaVerificationStepProps } from '@/routes/auth/login/type'

/** Login MFA ikinci adım — `name="mfaInput"` FormProvider üzerinden Zod’a bağlı. */
export function MfaVerificationStep({ mode, onToggleMode, onBack }: MfaVerificationStepProps) {
  const { t } = useLanguage()

  return (
    <div className="space-y-3">
      <FormFloatingInput<LoginFormValues>
        name="mfaInput"
        autoFocus
        autoComplete="one-time-code"
        maxLength={mode === 'TOTP' ? 6 : 8}
        inputMode={mode === 'TOTP' ? 'numeric' : 'text'}
        label={mode === 'TOTP' ? t('login.mfaTotpLabel') : t('login.mfaRecoveryLabel')}
        autoClear
        transformValue={(value) => (mode === 'RECOVERY' ? value.toUpperCase() : value)}
        className="[&_input]:text-center [&_input]:font-mono [&_input]:tracking-[0.35em]"
      />

      <div className="flex items-center justify-between gap-2">
        <button
          type="button"
          onClick={onBack}
          className="text-xs font-medium text-secondary transition-colors hover:text-primary"
        >
          ← {t('login.backToCredentials')}
        </button>
        <button
          type="button"
          onClick={onToggleMode}
          className="text-xs font-medium text-brand transition-colors hover:text-brand-hover"
        >
          {mode === 'TOTP' ? t('login.useRecoveryCode') : t('login.useTotpCode')}
        </button>
      </div>
    </div>
  )
}
