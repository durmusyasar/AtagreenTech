import { LoginOutlined, SafetyCertificateOutlined } from '@ant-design/icons'
import { Alert, App, Button, Card, Flex, Form, Input, Typography } from 'antd'
import { useState } from 'react'
import { Navigate, useLocation, useNavigate } from 'react-router-dom'
import { BrandMark } from '../../components/BrandMark'
import { ApiError } from '../../core/api/apiError'
import { useAuth } from '../../core/context/useAuth'
import { useLanguage } from '../../core/context/useLanguage'
import { useTenant } from '../../core/context/useTenant'
import { useAppMutation } from '../../core/hooks/useAppMutation'
import type { MessageKey } from '../../core/i18n/messages'
import { TENANT_SELECTION_ENABLED, tenantAddressPattern } from '../../core/tenant'
import { authService } from '../../services/authService'
import { AuthErrorCodes, type LoginRequest, type LoginResult } from '../../types/auth'

interface LoginForm {
  email: string
  password: string
  mfaCode?: string
}

/**
 * Gerçek kimlik girişi: `POST /auth/login` (+ isteğe bağlı MFA ikinci adım).
 * Tenant adresten (veya geliştirmede `VITE_DEV_TENANT`) gelir.
 */
export function LoginPage() {
  const { isAuthenticated } = useAuth()
  const { tenant, isTenantFromHost } = useTenant()
  const { t } = useLanguage()
  const { message } = App.useApp()
  const navigate = useNavigate()
  const from = (useLocation().state as { from?: string } | null)?.from ?? '/'
  const [mfaStepRequired, setMfaStepRequired] = useState(false)
  const [formError, setFormError] = useState<string>()
  const [form] = Form.useForm<LoginForm>()

  const login = useAppMutation<LoginResult, LoginRequest>({
    mutationFn: (request) => authService.login(request),
    onSuccess: (result) => {
      message.success(t('login.welcome', result.userName))
      navigate(from, { replace: true })
    },
    onError: (error) => {
      if (error instanceof ApiError && error.code === AuthErrorCodes.MfaRequired) {
        setMfaStepRequired(true)
        setFormError(t('login.mfaRequired'))
        return
      }
      setFormError(describeLoginError(error, t))
    },
  })

  if (isAuthenticated) return <Navigate to={from} replace />

  const tenantReady = isTenantFromHost || TENANT_SELECTION_ENABLED

  const submit = (values: LoginForm) => {
    setFormError(undefined)
    const mfaCode = values.mfaCode?.trim()
    const looksLikeTotp = mfaCode !== undefined && /^\d{6}$/.test(mfaCode)

    login.mutate({
      email: values.email,
      password: values.password,
      totpCode: mfaStepRequired && looksLikeTotp ? mfaCode : undefined,
      recoveryCode: mfaStepRequired && mfaCode && !looksLikeTotp ? mfaCode : undefined,
    })
  }

  return (
    <Flex align="center" justify="center" style={{ minHeight: '100vh', padding: 24 }}>
      <Card style={{ width: '100%', maxWidth: 480 }}>
        <Flex vertical gap={8} style={{ marginBottom: 24 }}>
          <BrandMark size={32} nameStyle={{ fontSize: 20, margin: 0 }} />
          <Typography.Text type="secondary">{t('login.description')}</Typography.Text>
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

            {!mfaStepRequired ? (
              <>
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
                >
                  <Input.Password autoComplete="current-password" placeholder="••••••••" />
                </Form.Item>
              </>
            ) : (
              <Form.Item
                name="mfaCode"
                label={t('login.mfaCode')}
                rules={[{ required: true, whitespace: true, message: t('login.mfaCodeRequired') }]}
              >
                <Input
                  autoFocus
                  autoComplete="one-time-code"
                  placeholder="123456"
                  prefix={<SafetyCertificateOutlined />}
                  style={{ letterSpacing: 4, textAlign: 'center', fontSize: 18 }}
                />
              </Form.Item>
            )}

            {formError && (
              <Alert type="error" showIcon style={{ marginBottom: 16 }} message={formError} />
            )}

            <Button
              type="primary"
              htmlType="submit"
              icon={<LoginOutlined />}
              loading={login.isPending}
              block
            >
              {mfaStepRequired ? t('login.verifyMfa') : t('login.submit')}
            </Button>

            {mfaStepRequired && (
              <Button
                type="link"
                block
                style={{ marginTop: 8 }}
                onClick={() => {
                  setMfaStepRequired(false)
                  setFormError(undefined)
                  form.setFieldValue('mfaCode', undefined)
                }}
              >
                {t('login.backToCredentials')}
              </Button>
            )}
          </Form>
        )}
      </Card>
    </Flex>
  )
}

function describeLoginError(error: unknown, t: (key: MessageKey, ...params: unknown[]) => string): string {
  if (error instanceof ApiError) {
    if (error.message && error.code !== `http_${error.status}`) return error.message
    return t('login.failed')
  }
  return t('login.failed')
}
