import { useMutation, useQueryClient } from '@tanstack/react-query'
import { App, Button, Flex, Form, Input, Modal, Switch } from 'antd'
import { useMemo } from 'react'
import { FormErrorAlert } from '../../components/forms/FormErrorAlert'
import { useServerErrors } from '../../components/forms/useServerErrors'
import { invalidate } from '../../core/api/queryClient'
import { useLanguage } from '../../core/context/useLanguage'
import { queryKeys } from '../../services/queryKeys'
import { warehouseService } from '../../services/warehouseService'
import type { UpdateWarehouseRequest, Warehouse } from '../../types/warehouse'
import { toFormValues, toRequest, WAREHOUSE_CODE_PATTERN, WAREHOUSE_LIMITS, type WarehouseFormValues } from './warehouseForm'

interface WarehouseFormModalProps {
  open: boolean
  /** Düzenlenecek depo; `null` ise yeni depo oluşturulur. */
  warehouse: Warehouse | null
  onClose: () => void
  onSaved: () => void
}

export function WarehouseFormModal({ open, warehouse, onClose, onSaved }: WarehouseFormModalProps) {
  const { t } = useLanguage()

  return (
    <Modal
      open={open}
      title={warehouse ? t('warehouse.editTitle', warehouse.code) : t('warehouse.createTitle')}
      footer={null}
      width={560}
      maskClosable={false}
      destroyOnHidden
      onCancel={onClose}
    >
      <WarehouseForm warehouse={warehouse} onCancel={onClose} onSaved={onSaved} />
    </Modal>
  )
}

function WarehouseForm({ warehouse, onCancel, onSaved }: Omit<WarehouseFormModalProps, 'open' | 'onClose'> & {
  onCancel: () => void
}) {
  const { t } = useLanguage()
  const { message } = App.useApp()
  const [form] = Form.useForm<WarehouseFormValues>()
  const initialValues = useMemo(() => toFormValues(warehouse), [warehouse])
  const queryClient = useQueryClient()
  const save = useMutation({
    // Hatalar global bildirim yerine formun içinde, ilgili alanın yanında gösterilir.
    mutationFn: ({ isActive, ...request }: UpdateWarehouseRequest): Promise<unknown> =>
      warehouse
        ? warehouseService.update(warehouse.id, { ...request, isActive }, { skipErrorNotification: true })
        : warehouseService.create(request, { skipErrorNotification: true }),
    // Hareketler ve bakiyeler depo kodunu ve adını gösterir.
    onSuccess: () => invalidate(queryClient, queryKeys.warehouses, queryKeys.stock),
  })
  const serverErrors = useServerErrors(form, {
    rootFields: ['code', 'name', 'address'],
    businessRuleFields: { 'Inventory.WarehouseCodeAlreadyExists': 'code' },
  })

  const submit = async (values: WarehouseFormValues) => {
    const request = toRequest(values)
    serverErrors.clearFormError()

    try {
      await save.mutateAsync({ ...request, isActive: values.isActive })
      message.success(t(warehouse ? 'warehouse.updated' : 'warehouse.created', request.code.toUpperCase()))
      onSaved()
    } catch (error) {
      serverErrors.applyServerError(error)
    }
  }

  return (
    <Form<WarehouseFormValues>
      form={form}
      layout="vertical"
      initialValues={initialValues}
      onFinish={submit}
      onFinishFailed={serverErrors.clearFormError}
    >
      <FormErrorAlert message={serverErrors.formError} onClose={serverErrors.clearFormError} />

      <Flex gap={16} align="start">
        <Form.Item
          name="code"
          label={t('warehouse.form.code')}
          extra={t('warehouse.form.codeHelp')}
          normalize={(value: string) => value.toUpperCase()}
          rules={[
            { required: true, whitespace: true, message: t('warehouse.form.codeRequired') },
            { pattern: WAREHOUSE_CODE_PATTERN, message: t('warehouse.form.codePattern') },
          ]}
          style={{ flex: 1 }}
        >
          <Input maxLength={WAREHOUSE_LIMITS.code} showCount autoFocus autoComplete="off" placeholder="MTR-01" />
        </Form.Item>

        {warehouse && (
          <Form.Item name="isActive" label={t('form.isActive')} valuePropName="checked">
            <Switch />
          </Form.Item>
        )}
      </Flex>

      <Form.Item
        name="name"
        label={t('warehouse.form.name')}
        rules={[{ required: true, whitespace: true, message: t('warehouse.form.nameRequired') }]}
      >
        <Input maxLength={WAREHOUSE_LIMITS.name} showCount autoComplete="off" placeholder={t('warehouse.form.namePlaceholder')} />
      </Form.Item>

      <Form.Item name="address" label={t('warehouse.form.address')}>
        <Input.TextArea maxLength={WAREHOUSE_LIMITS.address} showCount autoSize={{ minRows: 2, maxRows: 5 }} />
      </Form.Item>

      <Flex justify="end" gap={8}>
        <Button onClick={onCancel}>{t('common.cancel')}</Button>
        <Button type="primary" htmlType="submit" loading={save.isPending}>
          {t('common.save')}
        </Button>
      </Flex>
    </Form>
  )
}
