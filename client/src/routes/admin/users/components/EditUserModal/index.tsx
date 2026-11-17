import { App, Checkbox, Form, Input, Modal, Typography } from 'antd'
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
import { TENANT_ROLES } from '@/types/user'
import type { EditUserForm, EditUserModalProps } from '@/routes/admin/users/components/EditUserModal/type'

export function EditUserModal({ user, open, onClose, onSuccess }: EditUserModalProps) {
  const { t } = useLanguage()
  const { message } = App.useApp()
  const [form] = Form.useForm<EditUserForm>()
  const queryClient = useQueryClient()
  const serverErrors = useServerErrors(form, { rootFields: ['firstName', 'lastName', 'roles'] })

  useEffect(() => {
    if (!open || !user) return
    form.setFieldsValue({
      firstName: user.firstName,
      lastName: user.lastName,
      roles: user.roles,
    })
  }, [open, user, form])

  const update = useAppMutation({
    mutationFn: (values: EditUserForm) => {
      if (!user) return Promise.reject(new Error('user missing'))
      return userService.update(user.id, values)
    },
    onSuccess: async () => {
      message.success(t('users.updated'))
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
      title={t('users.editTitle')}
      okText={t('common.save')}
      cancelText={t('common.cancel')}
      confirmLoading={update.isPending}
      maskClosable={false}
      destroyOnHidden
      onCancel={onClose}
      onOk={() => form.submit()}
    >
      {user && (
        <Typography.Paragraph type="secondary" style={{ marginTop: 0 }}>
          {user.email}
        </Typography.Paragraph>
      )}

      <FormErrorAlert message={serverErrors.formError} onClose={serverErrors.clearFormError} />
      <Form<EditUserForm>
        form={form}
        layout="vertical"
        requiredMark={false}
        onFinish={(values) => {
          serverErrors.clearFormError()
          update.mutate(values)
        }}
        onFinishFailed={serverErrors.clearFormError}
      >
        <Form.Item
          name="firstName"
          label={t('invite.firstName')}
          rules={[{ required: true, whitespace: true, message: t('invite.firstNameRequired') }]}
        >
          <Input />
        </Form.Item>

        <Form.Item
          name="lastName"
          label={t('invite.lastName')}
          rules={[{ required: true, whitespace: true, message: t('invite.lastNameRequired') }]}
        >
          <Input />
        </Form.Item>

        <Form.Item
          name="roles"
          label={t('users.roles')}
          rules={[
            {
              validator: async (_, value: string[] | undefined) => {
                if (!value || value.length === 0) throw new Error(t('users.rolesRequired'))
              },
            },
          ]}
        >
          <Checkbox.Group
            options={TENANT_ROLES.map((role) => ({ value: role, label: role }))}
          />
        </Form.Item>
      </Form>
    </Modal>
  )
}
