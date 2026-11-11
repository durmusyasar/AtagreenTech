import { LoginOutlined } from '@ant-design/icons'
import { Button, Card, Flex, Form, Input, Typography } from 'antd'
import { Navigate, useLocation, useNavigate } from 'react-router-dom'
import { useAuth } from '../../core/context/useAuth'
import { useLanguage } from '../../core/context/useLanguage'
import { useTenant } from '../../core/context/useTenant'
import { isExpired, readTokenClaims } from '../../core/jwt'

interface LoginForm {
  token: string
  tenant?: string
}

/**
 * Kimlik servisi henüz olmadığından oturum, stok servisinin doğrulayacağı bir erişim token'ıyla açılır. Token'ın
 * imzası ve tenant'ı ilk API isteğinde doğrulanır; geçersizse 401 ile oturum kapanır.
 */
export function LoginPage() {
  const { isAuthenticated, signIn } = useAuth()
  const { tenant, isTenantFromHost } = useTenant()
  const { t } = useLanguage()
  const navigate = useNavigate()
  const from = (useLocation().state as { from?: string } | null)?.from ?? '/'

  if (isAuthenticated) return <Navigate to={from} replace />

  const submit = ({ token, tenant: formTenant }: LoginForm) => {
    signIn(token.trim(), formTenant)
    navigate(from, { replace: true })
  }

  return (
    <Flex align="center" justify="center" style={{ minHeight: '100vh', padding: 24 }}>
      <Card style={{ width: '100%', maxWidth: 480 }}>
        <Flex vertical gap={8} style={{ marginBottom: 24 }}>
          <Flex align="center" gap={12}>
            <img src="/favicon.svg" alt="" width={32} height={32} />
            <Typography.Title level={3} style={{ margin: 0 }}>
              {t('app.title')}
            </Typography.Title>
          </Flex>
          <Typography.Text type="secondary">{t('login.description')}</Typography.Text>
        </Flex>

        <Form<LoginForm> layout="vertical" requiredMark={false} initialValues={{ tenant }} onFinish={submit}>
          {isTenantFromHost ? (
            <Typography.Paragraph>{t('login.tenantFromHost', tenant)}</Typography.Paragraph>
          ) : (
            <Form.Item
              name="tenant"
              label={t('login.tenant')}
              rules={[{ required: true, whitespace: true, message: t('login.tenantRequired') }]}
            >
              <Input autoComplete="organization" />
            </Form.Item>
          )}

          <Form.Item
            name="token"
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
            <Input.TextArea rows={5} spellCheck={false} autoComplete="off" />
          </Form.Item>

          <Button type="primary" htmlType="submit" icon={<LoginOutlined />} block>
            {t('login.submit')}
          </Button>
        </Form>
      </Card>
    </Flex>
  )
}
