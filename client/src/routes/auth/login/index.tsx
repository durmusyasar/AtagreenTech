import { LoginOutlined } from '@ant-design/icons'
import { Alert, Button, Form, Input, Typography } from 'antd'
import { Link, Navigate } from 'react-router-dom'
import { PublicAuthLayout } from '@/Components/PublicAuthLayout'
import { useTenant } from '@/core/context/useTenant'
import { TENANT_SELECTION_ENABLED, tenantAddressPattern } from '@/core/tenant'
import { MfaVerificationStep } from '@/routes/auth/login/components/MfaVerificationStep'
import { useLoginForm } from '@/routes/auth/login/hooks/useLoginForm'
import type { LoginForm } from '@/routes/auth/login/type'

/**
 * Email/şifre + MFA ikinci adım giriş ekranı.
 * PublicAuthLayout ile Navbar/Footer; form kartı merkezi tasarım token'larını kullanır.
 */
export function LoginPage() {
  const { tenant, isTenantFromHost } = useTenant()
  const {
    isAuthenticated,
    from,
    invitedBanner,
    resetBanner,
    passwordChangedBanner,
    step,
    mfaMode,
    formError,
    form,
    login,
    resetToCredentials,
    switchMfaMode,
    submit,
    t,
  } = useLoginForm()

  if (isAuthenticated) return <Navigate to={from} replace />

  const tenantReady = isTenantFromHost || TENANT_SELECTION_ENABLED
  const tenantName =
    isTenantFromHost || TENANT_SELECTION_ENABLED ? (tenant ?? undefined) : undefined

  return (
    <PublicAuthLayout tenantName={tenantName}>
      <div className="w-full max-w-[420px] space-y-6 rounded-2xl border border-subtle bg-surface p-7 shadow-xl shadow-black/5 sm:p-8 dark:shadow-black/30">
        <div className="space-y-1.5">
          <div className="mb-1 inline-flex items-center gap-2 rounded-md bg-brand/10 px-2.5 py-1 text-xs font-medium text-brand">
            OmniSyncCore IAM
          </div>
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
            initialValues={{ email: '', password: '' }}
            className="space-y-1"
          >
            {isTenantFromHost ? (
              <Typography.Paragraph className="!mb-4 text-sm text-secondary">
                {t('login.tenantFromHost', tenant)}
              </Typography.Paragraph>
            ) : (
              <Typography.Paragraph type="secondary" className="!mb-4 text-sm">
                {t('login.devTenant', tenant)}
              </Typography.Paragraph>
            )}

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
                rules={[
                  { required: true, whitespace: true, message: t('login.emailRequired') },
                  { type: 'email', message: t('login.emailInvalid') },
                ]}
              >
                <Input
                  autoComplete="username"
                  placeholder="admin@omnisync.com"
                  className="!rounded-xl"
                  size="large"
                />
              </Form.Item>

              <div className="mb-1 flex items-center justify-between gap-2">
                <span className="text-sm text-secondary">{t('login.passwordLabel')}</span>
                <Link to="/forgot-password" className="text-xs font-medium text-brand hover:text-brand-hover">
                  {t('login.forgotPassword')}
                </Link>
              </div>
              <Form.Item
                name="password"
                rules={[{ required: true, message: t('login.passwordRequired') }]}
              >
                <Input.Password
                  autoComplete="current-password"
                  placeholder="••••••••"
                  className="!rounded-xl"
                  size="large"
                />
              </Form.Item>
            </div>

            {step === 'MFA' && (
              <MfaVerificationStep mode={mfaMode} onBack={resetToCredentials} onToggleMode={switchMfaMode} />
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
