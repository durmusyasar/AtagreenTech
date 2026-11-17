import { LoginOutlined } from '@ant-design/icons'
import { Alert, Button, Form, Input } from 'antd'
import { Link, Navigate } from 'react-router-dom'
import { PublicAuthLayout } from '@/Components/PublicAuthLayout'
import { useTenant } from '@/core/context/useTenant'
import { TENANT_SELECTION_ENABLED, tenantAddressPattern } from '@/core/tenant'
import { MfaVerificationStep } from '@/routes/auth/login/components/MfaVerificationStep'
import { useLoginForm } from '@/routes/auth/login/hooks/useLoginForm'
import type { LoginForm } from '@/routes/auth/login/type'

/**
 * Sade login kartı: Zod alan hataları + MFA; tenant/IAM rozeti yok.
 */
export function LoginPage() {
  const { isTenantFromHost } = useTenant()
  const {
    isAuthenticated,
    from,
    invitedBanner,
    resetBanner,
    passwordChangedBanner,
    step,
    mfaMode,
    formError,
    fieldErrors,
    form,
    login,
    resetToCredentials,
    switchMfaMode,
    submit,
    t,
  } = useLoginForm()

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
          <Alert
            type="warning"
            showIcon
            message={t(
              'login.tenantAddressRequired',
              tenantAddressPattern(window.location.hostname, window.location.port),
            )}
          />
        ) : (
          <Form<LoginForm>
            form={form}
            layout="vertical"
            requiredMark={false}
            onFinish={submit}
            initialValues={{ email: '', password: '', rememberMe: false }}
            noValidate
            className="space-y-1"
          >
            {invitedBanner && (
              <Alert type="success" showIcon className="!mb-4" message={t('login.invitedSuccess')} />
            )}
            {resetBanner && (
              <Alert type="success" showIcon className="!mb-4" message={t('login.resetSuccess')} />
            )}
            {passwordChangedBanner && (
              <Alert type="success" showIcon className="!mb-4" message={t('login.passwordChangedSuccess')} />
            )}

            <div style={{ display: step === 'CREDENTIALS' ? undefined : 'none' }}>
              <Form.Item
                name="email"
                label={t('login.emailLabel')}
                validateStatus={fieldErrors.email ? 'error' : undefined}
                help={fieldErrors.email ? t(fieldErrors.email) : undefined}
              >
                <Input
                  autoComplete="username"
                  placeholder="admin@omnisync.com"
                  className="!rounded-xl"
                  size="large"
                />
              </Form.Item>

              <Form.Item
                name="password"
                label={t('login.passwordLabel')}
                validateStatus={fieldErrors.password ? 'error' : undefined}
                help={fieldErrors.password ? t(fieldErrors.password) : undefined}
              >
                <Input.Password
                  autoComplete="current-password"
                  placeholder="••••••••"
                  className="!rounded-xl"
                  size="large"
                />
              </Form.Item>

              <div className="flex items-center justify-between gap-2 pt-1 pb-2">
                <label className="flex cursor-pointer items-center gap-2.5 group">
                  <Form.Item name="rememberMe" valuePropName="checked" noStyle>
                    <input
                      type="checkbox"
                      className="h-4 w-4 cursor-pointer rounded border-subtle bg-elevated text-brand transition focus:ring-brand/30"
                    />
                  </Form.Item>
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
            </div>

            {step === 'MFA' && (
              <MfaVerificationStep
                mode={mfaMode}
                onBack={resetToCredentials}
                onToggleMode={switchMfaMode}
                error={fieldErrors.mfaInput ? t(fieldErrors.mfaInput) : undefined}
              />
            )}

            {formError && (
              <Alert type="error" showIcon role="alert" className="!mb-4" message={formError} />
            )}

            <Button
              type="primary"
              htmlType="submit"
              icon={<LoginOutlined />}
              loading={login.isPending}
              block
              size="large"
              className="!mt-2 !rounded-xl !font-medium"
            >
              {login.isPending
                ? t('login.verifying')
                : step === 'MFA'
                  ? t('login.verifySubmit')
                  : t('login.submit')}
            </Button>
          </Form>
        )}
      </div>
    </PublicAuthLayout>
  )
}
