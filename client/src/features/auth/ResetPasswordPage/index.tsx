import { Alert, Button, Form, Input, Typography } from 'antd'
import { useState } from 'react'
import { Link, useNavigate, useSearchParams } from 'react-router-dom'
import { ApiError } from '../../../core/api/apiError'
import { useLanguage } from '../../../core/context/useLanguage'
import { useAppMutation } from '../../../core/hooks/useAppMutation'
import { checkPasswordStrength } from '../../../core/passwordPolicy'
import { authPublicService } from '../../../services/authPublicService'
import { AuthPageShell } from '../AuthPageShell'
import { PasswordPolicyHints } from '../PasswordPolicyHints'
import type { ResetPasswordForm } from './type'

export function ResetPasswordPage() {
  const { t } = useLanguage()
  const navigate = useNavigate()
  const [searchParams] = useSearchParams()
  const token = searchParams.get('token')?.trim() ?? ''
  const [formError, setFormError] = useState<string>()
  const [password, setPassword] = useState('')
  const [form] = Form.useForm<ResetPasswordForm>()

  const reset = useAppMutation({
    mutationFn: authPublicService.resetPassword,
    onSuccess: () => navigate('/login?reset=1', { replace: true }),
    onError: (error) => {
      setFormError(
        error instanceof ApiError && error.message ? error.message : t('reset.failed'),
      )
    },
  })

  const submit = (values: ResetPasswordForm) => {
    setFormError(undefined)
    if (!token) {
      setFormError(t('reset.missingToken'))
      return
    }
    if (!checkPasswordStrength(values.newPassword).isValid) {
      setFormError(t('password.policyFailed'))
      return
    }
    reset.mutate({ token, newPassword: values.newPassword })
  }

  return (
    <AuthPageShell title={t('reset.title')}>
      {!token && (
        <Alert type="error" showIcon style={{ marginBottom: 16 }} message={t('reset.missingToken')} />
      )}

      <Form<ResetPasswordForm>
        form={form}
        layout="vertical"
        requiredMark={false}
        onFinish={submit}
        initialValues={{ newPassword: '', confirmPassword: '' }}
      >
        <Form.Item
          name="newPassword"
          label={t('reset.newPassword')}
          rules={[{ required: true, message: t('login.passwordRequired') }]}
        >
          <Input.Password
            autoComplete="new-password"
            onChange={(e) => setPassword(e.target.value)}
          />
        </Form.Item>
        <PasswordPolicyHints password={password} />

        <Form.Item
          name="confirmPassword"
          label={t('reset.confirmPassword')}
          dependencies={['newPassword']}
          rules={[
            { required: true, message: t('invite.confirmPasswordRequired') },
            ({ getFieldValue }) => ({
              validator(_, value) {
                if (!value || getFieldValue('newPassword') === value) return Promise.resolve()
                return Promise.reject(new Error(t('password.mismatch')))
              },
            }),
          ]}
          style={{ marginTop: 16 }}
        >
          <Input.Password autoComplete="new-password" />
        </Form.Item>

        {formError && (
          <Alert type="error" showIcon role="alert" style={{ marginBottom: 16 }} message={formError} />
        )}

        <Button type="primary" htmlType="submit" loading={reset.isPending} disabled={!token} block>
          {reset.isPending ? t('reset.submitting') : t('reset.submit')}
        </Button>

        <Typography.Paragraph style={{ marginTop: 16, marginBottom: 0, textAlign: 'center' }}>
          <Link to="/login">{t('auth.backToLogin')}</Link>
        </Typography.Paragraph>
      </Form>
    </AuthPageShell>
  )
}
