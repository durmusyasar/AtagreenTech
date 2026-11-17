import { useState } from 'react'
import { Link } from 'react-router-dom'
import { AuthPageShell } from '@/Components/AuthPageShell'
import { FloatingInput } from '@/Components/FloatingInput'
import { PasswordStrengthList } from '@/Components/PasswordStrengthList'
import { PasswordVisibilityToggle } from '@/Components/forms/PasswordVisibilityToggle'
import { useAcceptInvite } from '@/routes/auth/accept-invite/hooks/useAcceptInvite'

export function AcceptInvitePage() {
  const {
    t,
    token,
    firstName,
    setFirstName,
    lastName,
    setLastName,
    password,
    setPassword,
    confirmPassword,
    setConfirmPassword,
    fieldErrors,
    serverError,
    isSubmitting,
    handleSubmit,
  } = useAcceptInvite()
  const [showPass, setShowPass] = useState(false)

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

      <form onSubmit={handleSubmit} noValidate className="space-y-4">
        <div className="grid grid-cols-1 gap-3.5 sm:grid-cols-2">
          <FloatingInput
            id="invite-first-name"
            type="text"
            autoComplete="given-name"
            label={t('invite.firstName')}
            value={firstName}
            onChange={(event) => setFirstName(event.target.value)}
            error={fieldErrors.firstName ? t(fieldErrors.firstName) : undefined}
            disabled={!token}
          />
          <FloatingInput
            id="invite-last-name"
            type="text"
            autoComplete="family-name"
            label={t('invite.lastName')}
            value={lastName}
            onChange={(event) => setLastName(event.target.value)}
            error={fieldErrors.lastName ? t(fieldErrors.lastName) : undefined}
            disabled={!token}
          />
        </div>

        <div className="space-y-2">
          <FloatingInput
            id="invite-password"
            type={showPass ? 'text' : 'password'}
            autoComplete="new-password"
            label={t('invite.password')}
            value={password}
            onChange={(event) => setPassword(event.target.value)}
            error={fieldErrors.password ? t(fieldErrors.password) : undefined}
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
          <PasswordStrengthList password={password} />
        </div>

        <FloatingInput
          id="invite-confirm-password"
          type={showPass ? 'text' : 'password'}
          autoComplete="new-password"
          label={t('invite.confirmPassword')}
          value={confirmPassword}
          onChange={(event) => setConfirmPassword(event.target.value)}
          error={fieldErrors.confirmPassword ? t(fieldErrors.confirmPassword) : undefined}
          disabled={!token}
        />

        <button
          type="submit"
          disabled={isSubmitting || !token}
          className="mt-2 w-full rounded-xl bg-brand px-4 py-2.5 text-sm font-medium text-white shadow-sm shadow-brand/25 transition-all duration-150 hover:bg-brand-hover disabled:opacity-50"
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
    </AuthPageShell>
  )
}
