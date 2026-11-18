import { useState } from 'react'
import { Link } from 'react-router-dom'
import { AuthPageShell } from '@/Components/AuthPageShell'
import { FloatingInput } from '@/Components/FloatingInput'
import { PasswordStrengthList } from '@/Components/PasswordStrengthList'
import { PasswordVisibilityToggle } from '@/Components/forms/PasswordVisibilityToggle'
import { useResetPassword } from '@/routes/auth/reset-password/hooks/useResetPassword'

export function ResetPasswordPage() {
  const {
    t,
    token,
    newPassword,
    setNewPassword,
    confirmPassword,
    setConfirmPassword,
    fieldErrors,
    serverError,
    isSubmitting,
    handleSubmit,
  } = useResetPassword()
  const [showPass, setShowPass] = useState(false)

  return (
    <AuthPageShell title={t('reset.title')}>
      {!token ? (
        <div
          role="alert"
          className="mb-4 rounded-xl border border-red-500/30 bg-red-500/10 p-3.5 text-xs text-red-600 dark:text-red-400"
        >
          {t('reset.missingToken')}
        </div>
      ) : null}

      {serverError ? (
        <div
          role="alert"
          className="mb-4 rounded-xl border border-red-500/30 bg-red-500/10 p-3.5 text-xs text-red-600 dark:text-red-400"
        >
          {serverError}
        </div>
      ) : null}

      <form onSubmit={handleSubmit} noValidate className="space-y-4">
        <div className="space-y-2">
          <FloatingInput
            id="new-password"
            type={showPass ? 'text' : 'password'}
            autoComplete="new-password"
            label={t('reset.newPassword')}
            value={newPassword}
            onChange={(event) => setNewPassword(event.target.value)}
            error={fieldErrors.newPassword ? t(fieldErrors.newPassword) : undefined}
            disabled={!token}
            endAdornment={
              <PasswordVisibilityToggle
                visible={showPass}
                onToggle={() => setShowPass((value) => !value)}
                showLabel={t('login.showPassword')}
                hideLabel={t('login.hidePassword')}
              />
            }
          />
          <PasswordStrengthList password={newPassword} />
        </div>

        <FloatingInput
          id="confirm-password"
          type={showPass ? 'text' : 'password'}
          autoComplete="new-password"
          label={t('reset.confirmPassword')}
          value={confirmPassword}
          onChange={(event) => setConfirmPassword(event.target.value)}
          error={fieldErrors.confirmPassword ? t(fieldErrors.confirmPassword) : undefined}
          disabled={!token}
        />

        <button
          type="submit"
          disabled={isSubmitting || !token}
          className="auth-submit mt-2 h-12 rounded-xl bg-brand px-4 text-sm font-semibold text-white shadow-[0_10px_24px_-12px_rgb(var(--brand-primary)/0.85)] transition-[background-color,box-shadow] duration-150 hover:bg-brand-hover disabled:opacity-50 disabled:shadow-none"
        >
          {isSubmitting ? t('reset.submitting') : t('reset.submit')}
        </button>

        <div className="pt-1 text-center">
          <Link
            to="/login"
            className="text-xs font-medium text-secondary transition-colors hover:text-primary"
          >
            ← {t('auth.backToLogin')}
          </Link>
        </div>
      </form>
    </AuthPageShell>
  )
}
