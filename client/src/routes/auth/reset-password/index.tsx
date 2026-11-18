import { useState } from 'react'
import { FormProvider, useWatch } from 'react-hook-form'
import { Link } from 'react-router-dom'
import { AuthPageShell } from '@/Components/AuthPageShell'
import { FormFloatingInput } from '@/Components/forms/FormFloatingInput'
import { PasswordVisibilityToggle } from '@/Components/forms/PasswordVisibilityToggle'
import { PasswordStrengthList } from '@/Components/PasswordStrengthList'
import { useResetPassword } from '@/routes/auth/reset-password/hooks/useResetPassword'
import type { ResetPasswordFormValues } from '@/routes/auth/reset-password/type'

export function ResetPasswordPage() {
  const { t, form, token, serverError, isSubmitting, onSubmit } = useResetPassword()
  const [showPass, setShowPass] = useState(false)
  const newPassword = useWatch({ control: form.control, name: 'newPassword' }) ?? ''

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

      <FormProvider {...form}>
        <form onSubmit={onSubmit} noValidate className="space-y-4">
          <div className="space-y-2">
            <FormFloatingInput<ResetPasswordFormValues>
              name="newPassword"
              type={showPass ? 'text' : 'password'}
              autoComplete="new-password"
              label={t('reset.newPassword')}
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

          <FormFloatingInput<ResetPasswordFormValues>
            name="confirmPassword"
            type={showPass ? 'text' : 'password'}
            autoComplete="new-password"
            label={t('reset.confirmPassword')}
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
      </FormProvider>
    </AuthPageShell>
  )
}
