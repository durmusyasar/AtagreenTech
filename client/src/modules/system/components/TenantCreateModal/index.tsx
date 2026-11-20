import { Form, Input, Modal } from 'antd'
import { useEffect } from 'react'
import { useLanguage } from '@/core/context/useLanguage'
import type {
  TenantCreateFormValues,
  TenantCreateModalProps,
} from '@modules/system/components/TenantCreateModal/type'

export function TenantCreateModal({ open, confirming, onClose, onSubmit }: TenantCreateModalProps) {
  const { t } = useLanguage()
  const [form] = Form.useForm<TenantCreateFormValues>()

  useEffect(() => {
    if (!open) form.resetFields()
  }, [open, form])

  return (
    <Modal
      title={t('tenants.createTitle')}
      open={open}
      onCancel={onClose}
      onOk={() => void form.submit()}
      confirmLoading={confirming}
      destroyOnHidden
      okText={t('tenants.create')}
    >
      <Form
        form={form}
        layout="vertical"
        onFinish={(values) =>
          onSubmit({
            name: values.name.trim(),
            slug: values.slug.trim().toLowerCase(),
            adminEmail: values.adminEmail?.trim() || undefined,
          })
        }
      >
        <Form.Item
          name="name"
          label={t('tenants.form.name')}
          rules={[{ required: true, message: t('tenants.form.nameRequired') }]}
        >
          <Input maxLength={128} autoFocus />
        </Form.Item>
        <Form.Item
          name="slug"
          label={t('tenants.form.slug')}
          extra={t('tenants.form.slugHint')}
          rules={[
            { required: true, message: t('tenants.form.slugRequired') },
            {
              pattern: /^[a-z0-9]([a-z0-9-]{0,61}[a-z0-9])?$/,
              message: t('tenants.form.slugInvalid'),
            },
          ]}
          normalize={(value: string) => value?.toLowerCase().replace(/[^a-z0-9-]/g, '')}
        >
          <Input maxLength={63} placeholder="acme" />
        </Form.Item>
        <Form.Item
          name="adminEmail"
          label={t('tenants.form.adminEmail')}
          extra={t('tenants.form.adminEmailHint')}
          rules={[{ type: 'email', message: t('tenants.form.adminEmailInvalid') }]}
        >
          <Input maxLength={256} placeholder="admin@acme.com" />
        </Form.Item>
      </Form>
    </Modal>
  )
}
