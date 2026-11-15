import { LoginOutlined } from '@ant-design/icons'
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
import { isExpired, readTokenClaims } from '../../core/jwt'
import { TENANT_SELECTION_ENABLED, tenantAddressPattern } from '../../core/tenant'
import { authService } from '../../services/authService'
import type { LoginRequest, LoginResult } from '../../types/auth'
import { defaultAccessToken } from './defaultAccessToken'

interface LoginForm {
  accessToken: string
}

/**
 * Kimlik servisi henüz olmadığından oturum, stok servisinin doğrulayacağı erişim token'ıyla açılır.
 * Tenant adresten (veya geliştirmede `VITE_DEV_TENANT`) gelir; formda tenant alanı yoktur.
 * `VITE_DEFAULT_ACCESS_TOKEN` tanımlıysa tek tıkla oturum açılır; aksi halde token alanı gösterilir.
 */
export function LoginPage() {
  const { isAuthenticated } = useAuth()
  const { tenant, isTenantFromHost } = useTenant()
  const { t } = useLanguage()
  const { message } = App.useApp()
  const navigate = useNavigate()
  const from = (useLocation().state as { from?: string } | null)?.from ?? '/'
  const presetToken = defaultAccessToken()
  const [useCustomToken, setUseCustomToken] = useState(!presetToken)
  const [formError, setFormError] = useState<string>()
  const [form] = Form.useForm<LoginForm>()

  const login = useAppMutation<LoginResult, LoginRequest>({
    mutationFn: (request) => authService.login(request),
    onSuccess: (result) => {
      message.success(t('login.welcome', result.userName))
      navigate(from, { replace: true })
    },
    onError: (error) => {
      setFormError(describeLoginError(error, t))
    },
  })

  if (isAuthenticated) return <Navigate to={from} replace />

  const tenantReady = isTenantFromHost || TENANT_SELECTION_ENABLED
  const showTokenField = useCustomToken || !presetToken

  const submit = (values: LoginForm) => {
    setFormError(undefined)
    const accessToken = showTokenField ? values.accessToken : presetToken
    login.mutate({ accessToken })
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
            initialValues={{ accessToken: presetToken }}
            onFinish={submit}
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

            {presetToken && !useCustomToken && (
              <Alert type="info" showIcon style={{ marginBottom: 16 }} message={t('login.defaultTokenReady')} />
            )}

            {showTokenField && (
              <Form.Item
                name="accessToken"
                label={t('login.token')}
                rules={[
                  { required: true, whitespace: true, message: t('login.tokenRequired') },
                  {
                    validator: (_, value?: string) => {
                      if (!value?.trim()) return Promise.resolve()
                      const claims = readTokenClaims(value)
                      if (!claims) return Promise.reject(new Error(t('login.tokenInvalid')))
                      if (isExpired(claims)) return Promise.reject(new Error(t('login.tokenExpired')))
                      return Promise.resolve()
                    },
                  },
                ]}
              >
                <Input.TextArea rows={4} spellCheck={false} autoComplete="off" placeholder={t('login.tokenPlaceholder')} />
              </Form.Item>
            )}

            {formError && (
              <Alert type="error" showIcon style={{ marginBottom: 16 }} message={formError} />
            )}

            <Button type="primary" htmlType="submit" icon={<LoginOutlined />} loading={login.isPending} block>
              {t('login.submit')}
            </Button>

            {presetToken && (
              <Button
                type="link"
                block
                style={{ marginTop: 8 }}
                onClick={() => {
                  setFormError(undefined)
                  setUseCustomToken((current) => !current)
                }}
              >
                {useCustomToken ? t('login.useDefaultToken') : t('login.useCustomToken')}
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
    if (error.code === 'invalid_token') return t('login.tokenInvalid')
    if (error.code === 'token_expired') return t('login.tokenExpired')
    if (error.message && error.message !== 'invalid_token' && error.message !== 'token_expired') return error.message
    return t('login.failed')
  }
  return t('login.failed')
}
