import { FloatingInput } from '@/Components/FloatingInput'
import { useLanguage } from '@/core/context/useLanguage'
import type { MfaVerificationStepProps } from '@/routes/auth/login/type'

/** Login MFA ikinci adım alanları (TOTP / kurtarma kodu); hatalar Zod'dan gelir. */
export function MfaVerificationStep({
  mode,
  value,
  onChange,
  onBlur,
  onToggleMode,
  onBack,
  error,
}: MfaVerificationStepProps) {
  const { t } = useLanguage()

  return (
    <div className="space-y-3">
      <FloatingInput
        id="login-mfa"
        autoFocus
        autoComplete="one-time-code"
        maxLength={mode === 'TOTP' ? 6 : 8}
        inputMode={mode === 'TOTP' ? 'numeric' : 'text'}
        label={mode === 'TOTP' ? t('login.mfaTotpLabel') : t('login.mfaRecoveryLabel')}
        value={value}
        onChange={(event) =>
          onChange(mode === 'RECOVERY' ? event.target.value.toUpperCase() : event.target.value)
        }
        onBlur={onBlur}
        error={error}
        autoClear
        clearLabel={t('common.clear')}
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
