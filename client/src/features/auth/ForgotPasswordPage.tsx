import { Alert, Button, Form, Input, Typography } from 'antd'
import { useState } from 'react'
import { Link } from 'react-router-dom'
import { useLanguage } from '../../core/context/useLanguage'
import { useAppMutation } from '../../core/hooks/useAppMutation'
import { authPublicService } from '../../services/authPublicService'
import { AuthPageShell } from './AuthPageShell'

export function ForgotPasswordPage() {
  const { t } = useLanguage()
  const [submittedEmail, setSubmittedEmail] = useState<string>()

  const forgot = useAppMutation({
    mutationFn: (email: string) => authPublicService.forgotPassword(email),
    onSuccess: (_data, email) => setSubmittedEmail(email),
    onError: (_error, email) => setSubmittedEmail(email),
  })

  return (
    <AuthPageShell title={t('forgot.title')} description={t('forgot.description')}>
      {submittedEmail ? (
        <>
          <Alert
            type="success"
            showIcon
            style={{ marginBottom: 16 }}
            message={t('forgot.sent', submittedEmail)}
          />
          <Typography.Paragraph style={{ marginBottom: 0, textAlign: 'center' }}>
            <Link to="/login">{t('auth.backToLogin')}</Link>
          </Typography.Paragraph>
        </>
      ) : (
        <Form
          layout="vertical"
          requiredMark={false}
          onFinish={(values: { email: string }) => forgot.mutate(values.email.trim())}
        >
          <Form.Item
            name="email"
            label={t('login.email')}
            rules={[
              { required: true, whitespace: true, message: t('login.emailRequired') },
              { type: 'email', message: t('login.emailInvalid') },
            ]}
          >
            <Input type="email" autoComplete="email" placeholder="admin@omnisync.com" />
          </Form.Item>

          <Button type="primary" htmlType="submit" loading={forgot.isPending} block>
            {forgot.isPending ? t('auth.sending') : t('forgot.submit')}
          </Button>

          <Typography.Paragraph style={{ marginTop: 16, marginBottom: 0, textAlign: 'center' }}>
            <Link to="/login">{t('auth.backToLogin')}</Link>
          </Typography.Paragraph>
        </Form>
      )}
    </AuthPageShell>
  )
}
