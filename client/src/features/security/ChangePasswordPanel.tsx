import { App, Button, Form, Input, Typography } from 'antd'
import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { FormErrorAlert } from '../../components/forms/FormErrorAlert'
import { ApiError } from '../../core/api/apiError'
import { useLanguage } from '../../core/context/useLanguage'
import { useAppMutation } from '../../core/hooks/useAppMutation'
import { checkPasswordStrength } from '../../core/passwordPolicy'
import { sessionStore } from '../../core/session'
import { accountSecurityService } from '../../services/accountSecurityService'
import { PasswordPolicyHints } from '../auth/PasswordPolicyHints'

interface ChangePasswordForm {
  currentPassword: string
  newPassword: string
  confirmPassword: string
}

export function ChangePasswordPanel() {
  const { t } = useLanguage()
  const { message } = App.useApp()
  const navigate = useNavigate()
  const [form] = Form.useForm<ChangePasswordForm>()
  const [newPassword, setNewPassword] = useState('')
  const [formError, setFormError] = useState<string>()

  const change = useAppMutation({
    mutationFn: accountSecurityService.changePassword,
    onSuccess: () => {
      message.success(t('security.passwordChanged'))
      // Backend tüm refresh token'ları iptal eder; yerel oturumu temizleyip yeniden girişe yönlendir.
      sessionStore.signOut()
      navigate('/login?passwordChanged=1', { replace: true })
    },
    onError: (error) => {
      setFormError(
        error instanceof ApiError && error.message
          ? error.message
          : t('security.passwordChangeFailed'),
      )
    },
  })

  return (
    <div style={{ maxWidth: 480 }}>
      <Typography.Title level={4} style={{ marginTop: 0 }}>
        {t('security.passwordTitle')}
      </Typography.Title>
      <Typography.Paragraph type="secondary">{t('security.passwordDescription')}</Typography.Paragraph>

      <FormErrorAlert message={formError} onClose={() => setFormError(undefined)} />

      <Form<ChangePasswordForm>
        form={form}
        layout="vertical"
        requiredMark={false}
        onFinish={(values) => {
          setFormError(undefined)
          if (!checkPasswordStrength(values.newPassword).isValid) {
            setFormError(t('password.policyFailed'))
            return
          }
          change.mutate({
            currentPassword: values.currentPassword,
            newPassword: values.newPassword,
          })
        }}
      >
        <Form.Item
          name="currentPassword"
          label={t('security.currentPassword')}
          rules={[{ required: true, message: t('login.passwordRequired') }]}
        >
          <Input.Password autoComplete="current-password" />
        </Form.Item>

        <Form.Item
          name="newPassword"
          label={t('security.newPassword')}
          rules={[{ required: true, message: t('login.passwordRequired') }]}
        >
          <Input.Password
            autoComplete="new-password"
            onChange={(e) => setNewPassword(e.target.value)}
          />
        </Form.Item>
        <PasswordPolicyHints password={newPassword} />

        <Form.Item
          name="confirmPassword"
          label={t('security.confirmPassword')}
          dependencies={['newPassword']}
          style={{ marginTop: 16 }}
          rules={[
            { required: true, message: t('invite.confirmPasswordRequired') },
            ({ getFieldValue }) => ({
              validator(_, value) {
                if (!value || getFieldValue('newPassword') === value) return Promise.resolve()
                return Promise.reject(new Error(t('password.mismatch')))
              },
            }),
          ]}
        >
          <Input.Password autoComplete="new-password" />
        </Form.Item>

        <Button type="primary" htmlType="submit" loading={change.isPending}>
          {change.isPending ? t('security.passwordUpdating') : t('security.passwordSubmit')}
        </Button>
      </Form>
    </div>
  )
}
