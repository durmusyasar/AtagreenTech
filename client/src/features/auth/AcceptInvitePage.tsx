import { Alert, Button, Col, Form, Input, Row, Typography } from 'antd'
import { useState } from 'react'
import { Link, useNavigate, useSearchParams } from 'react-router-dom'
import { ApiError } from '../../core/api/apiError'
import { useLanguage } from '../../core/context/useLanguage'
import { useAppMutation } from '../../core/hooks/useAppMutation'
import { checkPasswordStrength } from '../../core/passwordPolicy'
import { authPublicService } from '../../services/authPublicService'
import { AuthPageShell } from './AuthPageShell'
import { PasswordPolicyHints } from './PasswordPolicyHints'

interface AcceptInviteForm {
  firstName: string
  lastName: string
  password: string
  confirmPassword: string
}

export function AcceptInvitePage() {
  const { t } = useLanguage()
  const navigate = useNavigate()
  const [searchParams] = useSearchParams()
  const token = searchParams.get('token')?.trim() ?? ''
  const [formError, setFormError] = useState<string>()
  const [password, setPassword] = useState('')
  const [form] = Form.useForm<AcceptInviteForm>()

  const accept = useAppMutation({
    mutationFn: authPublicService.acceptInvite,
    onSuccess: () => navigate('/login?invited=1', { replace: true }),
    onError: (error) => {
      setFormError(
        error instanceof ApiError && error.message
          ? error.message
          : t('invite.failed'),
      )
    },
  })

  const submit = (values: AcceptInviteForm) => {
    setFormError(undefined)
    if (!token) {
      setFormError(t('invite.missingToken'))
      return
    }
    if (!checkPasswordStrength(values.password).isValid) {
      setFormError(t('password.policyFailed'))
      return
    }
    accept.mutate({
      token,
      firstName: values.firstName,
      lastName: values.lastName,
      password: values.password,
    })
  }

  return (
    <AuthPageShell title={t('invite.title')} description={t('invite.description')}>
      {!token && (
        <Alert type="error" showIcon style={{ marginBottom: 16 }} message={t('invite.missingToken')} />
      )}

      <Form<AcceptInviteForm>
        form={form}
        layout="vertical"
        requiredMark={false}
        onFinish={submit}
        initialValues={{ firstName: '', lastName: '', password: '', confirmPassword: '' }}
      >
        <Row gutter={12}>
          <Col span={12}>
            <Form.Item
              name="firstName"
              label={t('invite.firstName')}
              rules={[{ required: true, whitespace: true, message: t('invite.firstNameRequired') }]}
            >
              <Input autoComplete="given-name" />
            </Form.Item>
          </Col>
          <Col span={12}>
            <Form.Item
              name="lastName"
              label={t('invite.lastName')}
              rules={[{ required: true, whitespace: true, message: t('invite.lastNameRequired') }]}
            >
              <Input autoComplete="family-name" />
            </Form.Item>
          </Col>
        </Row>

        <Form.Item
          name="password"
          label={t('invite.password')}
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
          label={t('invite.confirmPassword')}
          dependencies={['password']}
          rules={[
            { required: true, message: t('invite.confirmPasswordRequired') },
            ({ getFieldValue }) => ({
              validator(_, value) {
                if (!value || getFieldValue('password') === value) return Promise.resolve()
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

        <Button type="primary" htmlType="submit" loading={accept.isPending} disabled={!token} block>
          {accept.isPending ? t('invite.submitting') : t('invite.submit')}
        </Button>

        <Typography.Paragraph style={{ marginTop: 16, marginBottom: 0, textAlign: 'center' }}>
          <Link to="/login">{t('auth.backToLogin')}</Link>
        </Typography.Paragraph>
      </Form>
    </AuthPageShell>
  )
}
