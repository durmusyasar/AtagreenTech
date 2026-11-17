import { Alert, Button, Form, Input, Spin, Typography } from 'antd'
import { useEffect, useState } from 'react'
import { Link, useSearchParams } from 'react-router-dom'
import { ApiError } from '@/core/api/apiError'
import { useLanguage } from '@/core/context/useLanguage'
import { useAppMutation } from '@/core/hooks/useAppMutation'
import { authPublicService } from '@/services/authPublicService'
import { AuthPageShell } from '@/Components/AuthPageShell'
import type { VerifyStatus } from '@/routes/auth/verify-email/type'

export function VerifyEmailPage() {
  const { t } = useLanguage()
  const [searchParams] = useSearchParams()
  const token = searchParams.get('token')?.trim() || null

  const [status, setStatus] = useState<VerifyStatus>(token ? 'VERIFYING' : 'RESEND')
  const [banner, setBanner] = useState<string>()

  useEffect(() => {
    if (!token) return
    let cancelled = false

    void authPublicService
      .verifyEmail(token)
      .then(() => {
        if (cancelled) return
        setStatus('SUCCESS')
        setBanner(t('verify.success'))
      })
      .catch((error: unknown) => {
        if (cancelled) return
        setStatus('ERROR')
        setBanner(
          error instanceof ApiError && error.message ? error.message : t('verify.failed'),
        )
      })

    return () => {
      cancelled = true
    }
  }, [token, t])

  const resend = useAppMutation({
    mutationFn: (email: string) => authPublicService.resendVerification(email),
    onSuccess: () => setBanner(t('verify.resendSent')),
    onError: () => setBanner(t('verify.resendSent')),
  })

  return (
    <AuthPageShell title={t('verify.title')}>
      {status === 'VERIFYING' && (
        <div style={{ textAlign: 'center', padding: '24px 0' }}>
          <Spin />
          <Typography.Paragraph type="secondary" style={{ marginTop: 16 }}>
            {t('verify.verifying')}
          </Typography.Paragraph>
        </div>
      )}

      {status === 'SUCCESS' && (
        <>
          {banner && (
            <Alert type="success" showIcon style={{ marginBottom: 16 }} message={banner} />
          )}
          <Link to="/login">
            <Button type="primary" block>
              {t('login.submit')}
            </Button>
          </Link>
        </>
      )}

      {(status === 'ERROR' || status === 'RESEND') && (
        <>
          {banner && (
            <Alert
              type={status === 'ERROR' ? 'warning' : 'info'}
              showIcon
              style={{ marginBottom: 16 }}
              message={banner}
            />
          )}

          <Form
            layout="vertical"
            requiredMark={false}
            onFinish={(values: { email: string }) => {
              setBanner(undefined)
              resend.mutate(values.email.trim())
            }}
          >
            <Form.Item
              name="email"
              label={t('verify.resendLabel')}
              rules={[
                { required: true, whitespace: true, message: t('login.emailRequired') },
                { type: 'email', message: t('login.emailInvalid') },
              ]}
            >
              <Input type="email" autoComplete="email" placeholder="ornek@omnisync.com" />
            </Form.Item>

            <Button type="default" htmlType="submit" loading={resend.isPending} block>
              {resend.isPending ? t('auth.sending') : t('verify.resendSubmit')}
            </Button>
          </Form>

          <Typography.Paragraph style={{ marginTop: 16, marginBottom: 0, textAlign: 'center' }}>
            <Link to="/login">{t('auth.backToLogin')}</Link>
          </Typography.Paragraph>
        </>
      )}
    </AuthPageShell>
  )
}
