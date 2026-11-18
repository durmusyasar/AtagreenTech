import { useState } from 'react'
import { Link, Navigate } from 'react-router-dom'
import { FloatingInput } from '@/Components/FloatingInput'
import { PasswordVisibilityToggle } from '@/Components/forms/PasswordVisibilityToggle'
import { PublicAuthLayout } from '@/Components/PublicAuthLayout'
import { useTenant } from '@/core/context/useTenant'
import { TENANT_SELECTION_ENABLED, tenantAddressPattern } from '@/core/tenant'
import { MfaVerificationStep } from '@/routes/auth/login/components/MfaVerificationStep'
import { useLoginForm } from '@/routes/auth/login/hooks/useLoginForm'

/**
 * Sade login kartı: FloatingInput + Zod alan hataları + MFA; tenant/IAM rozeti yok.
 */
export function LoginPage() {
  const { isTenantFromHost } = useTenant()
  const {
    isAuthenticated,
    from,
    invitedBanner,
    resetBanner,
    passwordChangedBanner,
    email,
    setEmail,
    password,
    setPassword,
    rememberMe,
    setRememberMe,
    mfaInput,
    setMfaInput,
    step,
    mfaMode,
    formError,
    fieldErrors,
    isSubmitting,
    resetToCredentials,
    switchMfaMode,
    handleSubmit,
    t,
  } = useLoginForm()
  const [showPassword, setShowPassword] = useState(false)

  if (isAuthenticated) return <Navigate to={from} replace />

  const tenantReady = isTenantFromHost || TENANT_SELECTION_ENABLED

  return (
    <PublicAuthLayout>
      <div className="w-full max-w-[420px] space-y-6 rounded-2xl border border-subtle bg-surface p-7 shadow-xl shadow-slate-900/10 ring-1 ring-slate-900/5 sm:p-8 dark:shadow-black/40 dark:ring-white/5">
        <div className="space-y-1.5">
          <h1 className="text-2xl font-semibold tracking-tight text-primary">
            {step === 'CREDENTIALS' ? t('login.title') : t('login.mfaSubtitle')}
          </h1>
          <p className="text-sm leading-relaxed text-secondary">
            {step === 'CREDENTIALS'
              ? t('login.subtitle')
              : mfaMode === 'TOTP'
                ? t('login.totpPrompt')
                : t('login.recoveryPrompt')}
          </p>
        </div>

        {!tenantReady ? (
          <div
            role="alert"
            className="rounded-xl border border-amber-500/30 bg-amber-500/10 p-3.5 text-xs leading-relaxed text-amber-700 dark:text-amber-300"
          >
            {t(
              'login.tenantAddressRequired',
              tenantAddressPattern(window.location.hostname, window.location.port),
            )}
          </div>
        ) : (
          <form onSubmit={handleSubmit} noValidate className="space-y-4">
            {invitedBanner ? (
              <div className="rounded-xl border border-brand/30 bg-brand/10 p-3.5 text-xs leading-relaxed text-brand">
                {t('login.invitedSuccess')}
              </div>
            ) : null}
            {resetBanner ? (
              <div className="rounded-xl border border-brand/30 bg-brand/10 p-3.5 text-xs leading-relaxed text-brand">
                {t('login.resetSuccess')}
              </div>
            ) : null}
            {passwordChangedBanner ? (
              <div className="rounded-xl border border-brand/30 bg-brand/10 p-3.5 text-xs leading-relaxed text-brand">
                {t('login.passwordChangedSuccess')}
              </div>
            ) : null}

            {step === 'CREDENTIALS' ? (
              <>
                <FloatingInput
                  id="login-email"
                  type="email"
                  autoComplete="username"
                  label={t('login.emailLabel')}
                  value={email}
                  onChange={(event) => setEmail(event.target.value)}
                  error={fieldErrors.email ? t(fieldErrors.email) : undefined}
                />

                <FloatingInput
                  id="login-password"
                  type={showPassword ? 'text' : 'password'}
                  autoComplete="current-password"
                  label={t('login.passwordLabel')}
                  value={password}
                  onChange={(event) => setPassword(event.target.value)}
                  error={fieldErrors.password ? t(fieldErrors.password) : undefined}
                  endAdornment={
                    <PasswordVisibilityToggle
                      visible={showPassword}
                      onToggle={() => setShowPassword((value) => !value)}
                      showLabel={t('login.showPassword')}
                      hideLabel={t('login.hidePassword')}
                    />
                  }
                />

                <div className="flex items-center justify-between gap-2 pt-1">
                  <label className="group flex cursor-pointer items-center gap-2.5">
                    <input
                      type="checkbox"
                      checked={rememberMe}
                      onChange={(event) => setRememberMe(event.target.checked)}
                      className="h-4 w-4 cursor-pointer rounded border-subtle bg-elevated text-brand transition focus:ring-brand/30"
                    />
                    <span className="text-xs font-medium text-secondary transition-colors group-hover:text-primary">
                      {t('login.rememberMe')}
                    </span>
                  </label>

                  <Link
                    to="/forgot-password"
                    className="text-xs font-medium text-brand transition-colors hover:text-brand-hover"
                  >
                    {t('login.forgotPassword')}
                  </Link>
                </div>
              </>
            ) : (
              <MfaVerificationStep
                mode={mfaMode}
                value={mfaInput}
                onChange={setMfaInput}
                onBack={resetToCredentials}
                onToggleMode={switchMfaMode}
                error={fieldErrors.mfaInput ? t(fieldErrors.mfaInput) : undefined}
              />
            )}

            {formError ? (
              <div
                role="alert"
                className="rounded-xl border border-red-500/30 bg-red-500/10 p-3.5 text-xs text-red-600 dark:text-red-400"
              >
                {formError}
              </div>
            ) : null}

            <button
              type="submit"
              disabled={isSubmitting}
              className="mt-2 w-full rounded-xl bg-brand px-4 py-2.5 text-sm font-medium text-white shadow-sm shadow-brand/25 transition-all duration-150 hover:bg-brand-hover disabled:opacity-50"
            >
              {isSubmitting
                ? t('login.verifying')
                : step === 'MFA'
                  ? t('login.verifySubmit')
                  : t('login.submit')}
            </button>
          </form>
        )}
      </div>
    </PublicAuthLayout>
  )
}
