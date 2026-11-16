import { LoginOutlined, SafetyCertificateOutlined } from '@ant-design/icons'
import { Alert, App, Button, Card, Flex, Form, Input, Select, Typography } from 'antd'
import { useState } from 'react'
import { Navigate, useLocation, useNavigate } from 'react-router-dom'
import { BrandMark } from '../../components/BrandMark'
import { ApiError } from '../../core/api/apiError'
import { useAuth } from '../../core/context/useAuth'
import { useLanguage } from '../../core/context/useLanguage'
import { useTenant } from '../../core/context/useTenant'
import { useAppMutation } from '../../core/hooks/useAppMutation'
import { resolveAuthErrorMessage } from '../../core/i18n/authErrors'
import { LANGUAGE_LABELS, SUPPORTED_LANGUAGES, type Language } from '../../core/i18n/languages'
import { TENANT_SELECTION_ENABLED, tenantAddressPattern } from '../../core/tenant'
import { authService } from '../../services/authService'
import { AuthErrorCodes, type LoginRequest, type LoginResult } from '../../types/auth'

type LoginStep = 'CREDENTIALS' | 'MFA'
type MfaMode = 'TOTP' | 'RECOVERY'

interface LoginForm {
  email: string
  password: string
  mfaInput?: string
}

/**
 * Email/şifre + MFA ikinci adım (TOTP / kurtarma kodu) giriş ekranı.
 * Hata kodları `resolveAuthErrorMessage` ile TR/EN kullanıcı mesajına çevrilir.
 */
export function LoginPage() {
  const { isAuthenticated } = useAuth()
  const { tenant, isTenantFromHost } = useTenant()
  const { language, setLanguage, t } = useLanguage()
  const { message } = App.useApp()
  const navigate = useNavigate()
  const from = (useLocation().state as { from?: string } | null)?.from ?? '/'

  const [step, setStep] = useState<LoginStep>('CREDENTIALS')
  const [mfaMode, setMfaMode] = useState<MfaMode>('TOTP')
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
        setStep('MFA')
        setMfaMode('TOTP')
        setFormError(undefined)
        form.setFieldValue('mfaInput', undefined)
        return
      }

      if (step === 'MFA' && error instanceof ApiError && error.status === 401) {
        setFormError(
          resolveAuthErrorMessage(
            error.code === AuthErrorCodes.MfaInvalid ? error : AuthErrorCodes.MfaInvalid,
            language,
          ),
        )
        return
      }

      setFormError(resolveAuthErrorMessage(error, language))
    },
  })

  if (isAuthenticated) return <Navigate to={from} replace />

  const tenantReady = isTenantFromHost || TENANT_SELECTION_ENABLED

  const resetToCredentials = () => {
    setStep('CREDENTIALS')
    setMfaMode('TOTP')
    setFormError(undefined)
    form.setFieldValue('mfaInput', undefined)
  }

  const switchMfaMode = () => {
    setMfaMode((current) => (current === 'TOTP' ? 'RECOVERY' : 'TOTP'))
    setFormError(undefined)
    form.setFieldValue('mfaInput', undefined)
  }

  const submit = (values: LoginForm) => {
    setFormError(undefined)

    const request: LoginRequest = {
      email: values.email.trim(),
      password: values.password,
    }

    if (step === 'MFA') {
      const raw = values.mfaInput?.trim() ?? ''
      if (mfaMode === 'TOTP') {
        request.totpCode = raw
      } else {
        request.recoveryCode = raw.toUpperCase()
      }
    }

    login.mutate(request)
  }

  return (
    <Flex align="center" justify="center" style={{ minHeight: '100vh', padding: 24 }}>
      <Card style={{ width: '100%', maxWidth: 480 }}>
        <Flex align="flex-start" justify="space-between" gap={12} style={{ marginBottom: 24 }}>
          <Flex vertical gap={8} style={{ minWidth: 0 }}>
            <BrandMark size={32} nameStyle={{ fontSize: 20, margin: 0 }} />
            <Typography.Text type="secondary">
              {step === 'CREDENTIALS' ? t('login.description') : t('login.mfaTitle')}
            </Typography.Text>
          </Flex>
          <Select<Language>
            size="small"
            aria-label={t('header.language')}
            value={language}
            onChange={setLanguage}
            options={SUPPORTED_LANGUAGES.map((code) => ({ value: code, label: LANGUAGE_LABELS[code] }))}
            style={{ width: 120, flexShrink: 0 }}
          />
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

            {/* Credentials alanları MFA adımında da form state'te kalsın diye gizli tutulur. */}
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
              >
                <Input.Password autoComplete="current-password" placeholder="••••••••" />
              </Form.Item>
            </div>

            {step === 'MFA' && (
              <>
                <Typography.Paragraph type="secondary" style={{ marginBottom: 12 }}>
                  {mfaMode === 'TOTP' ? t('login.mfaTotpHint') : t('login.mfaRecoveryHint')}
                </Typography.Paragraph>

                <Form.Item
                  name="mfaInput"
                  label={mfaMode === 'TOTP' ? t('login.mfaTotpLabel') : t('login.mfaRecoveryLabel')}
                  rules={[
                    { required: true, whitespace: true, message: t('login.mfaCodeRequired') },
                    {
                      validator: async (_, value: string | undefined) => {
                        const raw = value?.trim() ?? ''
                        if (!raw) return
                        if (mfaMode === 'TOTP' && !/^\d{6}$/.test(raw)) {
                          throw new Error(t('login.mfaTotpInvalid'))
                        }
                        if (mfaMode === 'RECOVERY' && !/^[A-Za-z0-9]{8}$/.test(raw)) {
                          throw new Error(t('login.mfaRecoveryInvalid'))
                        }
                      },
                    },
                  ]}
                >
                  <Input
                    autoFocus
                    autoComplete="one-time-code"
                    maxLength={mfaMode === 'TOTP' ? 6 : 8}
                    placeholder={mfaMode === 'TOTP' ? '000000' : 'A1B2C3D4'}
                    prefix={<SafetyCertificateOutlined />}
                    style={{
                      letterSpacing: mfaMode === 'TOTP' ? 6 : 3,
                      textAlign: 'center',
                      fontSize: 18,
                      fontFamily: 'ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, monospace',
                      textTransform: mfaMode === 'RECOVERY' ? 'uppercase' : undefined,
                    }}
                  />
                </Form.Item>

                <Flex justify="space-between" style={{ marginBottom: 16 }}>
                  <Button type="link" style={{ paddingInline: 0 }} onClick={resetToCredentials}>
                    ← {t('login.backToCredentials')}
                  </Button>
                  <Button type="link" style={{ paddingInline: 0 }} onClick={switchMfaMode}>
                    {mfaMode === 'TOTP' ? t('login.useRecoveryCode') : t('login.useTotpCode')}
                  </Button>
                </Flex>
              </>
            )}

            {formError && (
              <Alert type="error" showIcon role="alert" style={{ marginBottom: 16 }} message={formError} />
            )}

            <Button
              type="primary"
              htmlType="submit"
              icon={<LoginOutlined />}
              loading={login.isPending}
              block
            >
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
