import { useState } from 'react'
import { FormProvider } from 'react-hook-form'
import { Link, Navigate, useSearchParams } from 'react-router-dom'
import { BrandMark } from '@/Components/BrandMark'
import { FormAuthCheckbox } from '@/Components/forms/FormAuthCheckbox'
import { FormFloatingInput } from '@/Components/forms/FormFloatingInput'
import { PasswordVisibilityToggle } from '@/Components/forms/PasswordVisibilityToggle'
import { PublicAuthLayout } from '@/Components/PublicAuthLayout'
import { useLanguage } from '@/core/context/useLanguage'
import { useTenant } from '@/core/context/useTenant'
import { TENANT_SELECTION_ENABLED, tenantAddressPattern } from '@/core/tenant'
import { MfaVerificationStep } from '@/routes/auth/login/components/MfaVerificationStep'
import {
  LOGIN_QUERY,
  LOGIN_QUERY_FLAG,
  LOGIN_STEP,
  MFA_MODE,
} from '@/routes/auth/login/constants'
import { useLoginForm } from '@/routes/auth/login/hooks/useLoginForm'
import type { LoginFormValues } from '@/routes/auth/login/type'

/**
 * Modern login: FormProvider + Zod; masaüstünde brand paneli + form.
 */
export function LoginPage() {
  const { t } = useLanguage()
  const { isTenantFromHost } = useTenant()
  const [searchParams] = useSearchParams()
  const {
    isAuthenticated,
    from,
    form,
    step,
    mfaMode,
    formError,
    isSubmitting,
    resetToCredentials,
    switchMfaMode,
    onSubmit,
  } = useLoginForm()
  const [showPassword, setShowPassword] = useState(false)

  if (isAuthenticated) return <Navigate to={from} replace />

  const tenantReady = isTenantFromHost || TENANT_SELECTION_ENABLED
  const invitedBanner = searchParams.get(LOGIN_QUERY.invited) === LOGIN_QUERY_FLAG
  const resetBanner = searchParams.get(LOGIN_QUERY.reset) === LOGIN_QUERY_FLAG
  const passwordChangedBanner =
    searchParams.get(LOGIN_QUERY.passwordChanged) === LOGIN_QUERY_FLAG

  return (
    <PublicAuthLayout>
      <div className="auth-panel auth-panel-rise grid max-w-[920px] lg:grid-cols-[1.05fr_1fr]">
        <aside className="relative hidden overflow-hidden border-b border-subtle/70 bg-gradient-to-br from-brand/[0.14] via-surface to-surface p-9 dark:border-white/10 dark:from-brand/[0.22] lg:flex lg:flex-col lg:justify-between lg:border-b-0 lg:border-r">
          <div aria-hidden className="pointer-events-none absolute inset-0">
            <div className="absolute -right-16 -top-10 h-56 w-56 rounded-full bg-brand/20 blur-3xl" />
            <div className="absolute -bottom-20 left-0 h-48 w-48 rounded-full bg-brand/10 blur-3xl" />
            <div
              className="absolute inset-0 opacity-40"
              style={{
                backgroundImage:
                  'radial-gradient(circle at 20% 20%, rgb(var(--brand-primary) / 0.18), transparent 42%), radial-gradient(circle at 80% 70%, rgb(var(--brand-primary) / 0.1), transparent 45%)',
              }}
            />
          </div>

          <div className="relative">
            <BrandMark size={42} nameClassName="text-[22px] leading-none" />
          </div>

          <div className="relative space-y-4 pt-16">
            <p className="max-w-[16ch] text-3xl font-semibold leading-tight tracking-tight text-primary">
              {t('login.panelHeadline')}
            </p>
            <p className="max-w-sm text-sm leading-relaxed text-secondary">{t('login.panelTagline')}</p>
          </div>

          <p className="relative pt-10 text-xs text-muted">{t('login.panelFootnote')}</p>
        </aside>

        <div className="relative flex flex-col justify-center p-7 sm:p-9 lg:p-10">
          <div className="mb-7 space-y-2 lg:mb-8">
            <div className="mb-5 lg:hidden">
              <BrandMark size={34} nameClassName="text-lg leading-none" />
            </div>
            <h1 className="auth-page-title">
              {step === LOGIN_STEP.Credentials ? t('login.title') : t('login.mfaSubtitle')}
            </h1>
            <p className="auth-page-description">
              {step === LOGIN_STEP.Credentials
                ? t('login.subtitle')
                : mfaMode === MFA_MODE.Totp
                  ? t('login.totpPrompt')
                  : t('login.recoveryPrompt')}
            </p>
          </div>

          {!tenantReady ? (
            <div
              role="alert"
              className="rounded-2xl border border-amber-500/30 bg-amber-500/10 p-3.5 text-xs leading-relaxed text-amber-700 dark:text-amber-300"
            >
              {t(
                'login.tenantAddressRequired',
                tenantAddressPattern(window.location.hostname, window.location.port),
              )}
            </div>
          ) : (
            <FormProvider {...form}>
              <form onSubmit={onSubmit} noValidate className="space-y-4">
                {invitedBanner ? (
                  <div className="rounded-2xl border border-brand/30 bg-brand/10 p-3.5 text-xs leading-relaxed text-brand">
                    {t('login.invitedSuccess')}
                  </div>
                ) : null}
                {resetBanner ? (
                  <div className="rounded-2xl border border-brand/30 bg-brand/10 p-3.5 text-xs leading-relaxed text-brand">
                    {t('login.resetSuccess')}
                  </div>
                ) : null}
                {passwordChangedBanner ? (
                  <div className="rounded-2xl border border-brand/30 bg-brand/10 p-3.5 text-xs leading-relaxed text-brand">
                    {t('login.passwordChangedSuccess')}
                  </div>
                ) : null}

                {step === LOGIN_STEP.Credentials ? (
                  <>
                    <FormFloatingInput<LoginFormValues>
                      name="email"
                      type="email"
                      autoComplete="username"
                      label={t('login.emailLabel')}
                      autoClear
                    />

                    <FormFloatingInput<LoginFormValues>
                      name="password"
                      type={showPassword ? 'text' : 'password'}
                      autoComplete="current-password"
                      label={t('login.passwordLabel')}
                      autoClear
                      endAdornment={
                        <PasswordVisibilityToggle
                          visible={showPassword}
                          onToggle={() => setShowPassword((value) => !value)}
                          showLabel={t('login.showPassword')}
                          hideLabel={t('login.hidePassword')}
                        />
                      }
                    />

                    <div className="flex items-center justify-between gap-3 pt-1.5">
                      <FormAuthCheckbox<LoginFormValues>
                        name="rememberMe"
                        id="login-remember-me"
                        label={t('login.rememberMe')}
                      />

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
                    onBack={resetToCredentials}
                    onToggleMode={switchMfaMode}
                    disabled={isSubmitting}
                  />
                )}

                {formError ? (
                  <div
                    role="alert"
                    className="rounded-2xl border border-red-500/30 bg-red-500/10 p-3.5 text-xs text-red-600 dark:text-red-400"
                  >
                    {formError}
                  </div>
                ) : null}

                <button
                  type="submit"
                  disabled={isSubmitting}
                  className="auth-submit mt-3 flex h-12 items-center justify-center rounded-xl bg-brand px-4 text-sm font-semibold text-white shadow-[0_10px_24px_-12px_rgb(var(--brand-primary)/0.85)] transition-[background-color,box-shadow,transform] duration-200 hover:bg-brand-hover hover:shadow-[0_14px_28px_-12px_rgb(var(--brand-primary)/0.9)] active:scale-[0.99] disabled:cursor-not-allowed disabled:opacity-50 disabled:shadow-none"
                >
                  {isSubmitting
                    ? t('login.verifying')
                    : step === LOGIN_STEP.Mfa
                      ? t('login.verifySubmit')
                      : t('login.submit')}
                </button>
              </form>
            </FormProvider>
          )}
        </div>
      </div>
    </PublicAuthLayout>
  )
}
