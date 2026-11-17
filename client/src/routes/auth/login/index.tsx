import { LoginOutlined } from '@ant-design/icons'
import { Alert, Button, Card, Flex, Form, Input, Typography } from 'antd'
import { Link, Navigate } from 'react-router-dom'
import { BrandMark } from '@/Components/BrandMark'
import { LanguageSwitcher } from '@/Components/LanguageSwitcher'
import { ThemeSwitcher } from '@/Components/ThemeSwitcher'
import { useTenant } from '@/core/context/useTenant'
import { TENANT_SELECTION_ENABLED, tenantAddressPattern } from '@/core/tenant'
import { MfaVerificationStep } from '@/routes/auth/login/components/MfaVerificationStep'
import { useLoginForm } from '@/routes/auth/login/hooks/useLoginForm'
import type { LoginForm } from '@/routes/auth/login/type'

/**
 * Email/şifre + MFA ikinci adım giriş ekranı.
 * Domain: `routes/auth/login` (hook + MFA alt bileşeni).
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

  return (
    <Flex
      align="center"
      justify="center"
      className="min-h-screen bg-slate-50 text-slate-900 dark:bg-slate-950 dark:text-slate-100"
      style={{ padding: 24 }}
    >
      <Card
        className="border-slate-200 bg-white shadow-sm dark:border-slate-800 dark:bg-slate-900"
        style={{ width: '100%', maxWidth: 480 }}
      >
        <Flex align="flex-start" justify="space-between" gap={12} style={{ marginBottom: 24 }}>
          <Flex vertical gap={8} style={{ minWidth: 0 }}>
            <BrandMark size={32} nameStyle={{ fontSize: 20, margin: 0 }} />
            <Typography.Text type="secondary">
              {step === 'CREDENTIALS' ? t('login.description') : t('login.mfaTitle')}
            </Typography.Text>
          </Flex>
          <Flex align="center" gap={2} style={{ flexShrink: 0 }}>
            <ThemeSwitcher variant="toggle" />
            <LanguageSwitcher size="small" />
          </Flex>
        </Flex>

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
          >
            {isTenantFromHost ? (
              <Typography.Paragraph style={{ marginBottom: 16 }}>
                {t('login.tenantFromHost', tenant)}
              </Typography.Paragraph>
            ) : (
              <Typography.Paragraph type="secondary" style={{ marginBottom: 16 }}>
                {t('login.devTenant', tenant)}
              </Typography.Paragraph>
            )}

            {invitedBanner && (
              <Alert type="success" showIcon style={{ marginBottom: 16 }} message={t('login.invitedSuccess')} />
            )}
            {resetBanner && (
              <Alert type="success" showIcon style={{ marginBottom: 16 }} message={t('login.resetSuccess')} />
            )}
            {passwordChangedBanner && (
              <Alert
                type="success"
                showIcon
                style={{ marginBottom: 16 }}
                message={t('login.passwordChangedSuccess')}
              />
            )}

            <div style={{ display: step === 'CREDENTIALS' ? undefined : 'none' }}>
              <Form.Item
                name="email"
                label={t('login.email')}
                rules={[
                  { required: true, whitespace: true, message: t('login.emailRequired') },
                  { type: 'email', message: t('login.emailInvalid') },
                ]}
              >
                <Input autoComplete="username" placeholder="admin@omnisync.com" />
              </Form.Item>

              <Form.Item
                name="password"
                label={t('login.password')}
                rules={[{ required: true, message: t('login.passwordRequired') }]}
                extra={
                  <Link to="/forgot-password" style={{ fontSize: 12 }}>
                    {t('login.forgotPassword')}
                  </Link>
                }
              >
                <Input.Password autoComplete="current-password" placeholder="••••••••" />
              </Form.Item>
            </div>

            {step === 'MFA' && (
              <MfaVerificationStep
                mode={mfaMode}
                onBack={resetToCredentials}
                onToggleMode={switchMfaMode}
              />
            )}

            {formError && (
              <Alert type="error" showIcon role="alert" style={{ marginBottom: 16 }} message={formError} />
            )}

            <Button type="primary" htmlType="submit" icon={<LoginOutlined />} loading={login.isPending} block>
              {login.isPending
                ? t('login.verifying')
                : step === 'MFA'
                  ? t('login.verifyMfa')
                  : t('login.submit')}
            </Button>
          </Form>
        )}
      </Card>
    </Flex>
  )
}
