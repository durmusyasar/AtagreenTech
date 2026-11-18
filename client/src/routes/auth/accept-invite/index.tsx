import { useState } from 'react'
import { FormProvider, useWatch } from 'react-hook-form'
import { Link } from 'react-router-dom'
import { AuthPageShell } from '@/Components/AuthPageShell'
import { FormFloatingInput } from '@/Components/forms/FormFloatingInput'
import { PasswordVisibilityToggle } from '@/Components/forms/PasswordVisibilityToggle'
import { PasswordStrengthList } from '@/Components/PasswordStrengthList'
import { useAcceptInvite } from '@/routes/auth/accept-invite/hooks/useAcceptInvite'
import type { AcceptInviteFormValues } from '@/routes/auth/accept-invite/type'

export function AcceptInvitePage() {
  const { t, form, token, serverError, isSubmitting, onSubmit } = useAcceptInvite()
  const [showPass, setShowPass] = useState(false)
  const password = useWatch({ control: form.control, name: 'password' }) ?? ''

  return (
    <AuthPageShell title={t('invite.title')} description={t('invite.description')}>
      {!token ? (
        <div
          role="alert"
          className="mb-4 rounded-xl border border-red-500/30 bg-red-500/10 p-3.5 text-xs text-red-600 dark:text-red-400"
        >
          {t('invite.missingToken')}
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
          <div className="grid grid-cols-1 gap-3.5 sm:grid-cols-2">
            <FormFloatingInput<AcceptInviteFormValues>
              name="firstName"
              type="text"
              autoComplete="given-name"
              label={t('invite.firstName')}
              disabled={!token}
              autoClear
            />
            <FormFloatingInput<AcceptInviteFormValues>
              name="lastName"
              type="text"
              autoComplete="family-name"
              label={t('invite.lastName')}
              disabled={!token}
              autoClear
            />
          </div>

          <div className="space-y-2">
            <FormFloatingInput<AcceptInviteFormValues>
              name="password"
              type={showPass ? 'text' : 'password'}
              autoComplete="new-password"
              label={t('invite.password')}
              disabled={!token}
              endAdornment={
                <PasswordVisibilityToggle
                  visible={showPass}
                  onToggle={() => setShowPass((value) => !value)}
                  showLabel={t('login.showPassword')}
                  hideLabel={t('login.hidePassword')}
                  disabled={!token}
                />
              }
            />
            <PasswordStrengthList password={password} />
          </div>

          <FormFloatingInput<AcceptInviteFormValues>
            name="confirmPassword"
            type={showPass ? 'text' : 'password'}
            autoComplete="new-password"
            label={t('invite.confirmPassword')}
            disabled={!token}
          />

          <button
            type="submit"
            disabled={isSubmitting || !token}
            className="auth-submit mt-2 h-12 rounded-xl bg-brand px-4 text-sm font-semibold text-white shadow-[0_10px_24px_-12px_rgb(var(--brand-primary)/0.85)] transition-[background-color,box-shadow] duration-150 hover:bg-brand-hover disabled:opacity-50 disabled:shadow-none"
          >
            {isSubmitting ? t('invite.submitting') : t('invite.submit')}
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
