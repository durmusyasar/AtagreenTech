import { App, Form, Input, Modal, Select } from 'antd'
import { useEffect } from 'react'
import { useQueryClient } from '@tanstack/react-query'
import { FormErrorAlert } from '@/Components/forms/FormErrorAlert'
import { useServerErrors } from '@/Components/forms/useServerErrors'
import { ApiError } from '@/core/api/apiError'
import { invalidate } from '@/core/api/queryClient'
import { useLanguage } from '@/core/context/useLanguage'
import { useAppMutation } from '@/core/hooks/useAppMutation'
import { queryKeys } from '@/services/queryKeys'
import { userService } from '@/services/userService'
import { TENANT_ROLES, type InviteUserRequest } from '@/types/user'
import type { InviteUserModalProps } from '@/routes/admin/users/components/InviteUserModal/type'

export function InviteUserModal({ open, onClose, onSuccess }: InviteUserModalProps) {
  const { t } = useLanguage()
  const { message } = App.useApp()
  const [form] = Form.useForm<InviteUserRequest>()
  const queryClient = useQueryClient()
  const serverErrors = useServerErrors(form, { rootFields: ['email', 'firstName', 'lastName', 'role'] })

  useEffect(() => {
    if (!open) return
    form.resetFields()
    form.setFieldsValue({ role: 'Viewer' })
  }, [open, form])

  const invite = useAppMutation({
    mutationFn: userService.invite,
    onSuccess: async () => {
      message.success(t('users.inviteSuccess'))
      await invalidate(queryClient, queryKeys.users)
      onSuccess()
      onClose()
    },
    onError: (error) => {
      if (error instanceof ApiError) serverErrors.applyServerError(error)
    },
  })

  return (
    <Modal
      open={open}
      title={t('users.inviteTitle')}
      okText={invite.isPending ? t('auth.sending') : t('users.inviteSubmit')}
      cancelText={t('common.cancel')}
      confirmLoading={invite.isPending}
      maskClosable={false}
      destroyOnHidden
      onCancel={onClose}
      onOk={() => form.submit()}
    >
      <FormErrorAlert message={serverErrors.formError} onClose={serverErrors.clearFormError} />
      <Form<InviteUserRequest>
        form={form}
        layout="vertical"
        requiredMark={false}
        initialValues={{ role: 'Viewer' }}
        onFinish={(values) => {
          serverErrors.clearFormError()
          invite.mutate(values)
        }}
        onFinishFailed={serverErrors.clearFormError}
      >
        <Form.Item
          name="email"
          label={t('login.email')}
          rules={[
            { required: true, whitespace: true, message: t('login.emailRequired') },
            { type: 'email', message: t('login.emailInvalid') },
          ]}
        >
          <Input autoComplete="off" placeholder="kullanici@omnisync.com" />
        </Form.Item>

        <Form.Item
          name="firstName"
          label={t('invite.firstName')}
          rules={[{ required: true, whitespace: true, message: t('invite.firstNameRequired') }]}
        >
          <Input autoComplete="off" />
        </Form.Item>

        <Form.Item
          name="lastName"
          label={t('invite.lastName')}
          rules={[{ required: true, whitespace: true, message: t('invite.lastNameRequired') }]}
        >
          <Input autoComplete="off" />
        </Form.Item>

        <Form.Item
          name="role"
          label={t('users.initialRole')}
          rules={[{ required: true, message: t('users.roleRequired') }]}
        >
          <Select
            options={TENANT_ROLES.map((role) => ({ value: role, label: role }))}
          />
        </Form.Item>
      </Form>
    </Modal>
  )
}
