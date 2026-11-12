import { ExportOutlined, ImportOutlined, SwapOutlined } from '@ant-design/icons'
import { Alert, App, Button, Col, Flex, Form, Input, InputNumber, Row, Segmented, type FormRule } from 'antd'
import { useState } from 'react'
import { Link } from 'react-router-dom'
import { FormErrorAlert } from '../../components/forms/FormErrorAlert'
import { useServerErrors } from '../../components/forms/useServerErrors'
import { useApiQuery } from '../../core/api/useApiQuery'
import { useLanguage } from '../../core/context/useLanguage'
import { stockService } from '../../services/stockService'
import type { Product } from '../../types/product'
import type { MovementType } from '../../types/stock'
import type { Warehouse } from '../../types/warehouse'
import {
  carryOverWarehouse,
  findWarehouseField,
  MOVEMENT_FIELDS,
  MOVEMENT_LIMITS,
  sourceWarehouseField,
  toMovementRequest,
  type MovementFormValues,
} from './movementRequest'
import { ProductSelect } from './ProductSelect'
import { decimalPlaces, formatQuantity, PRICE_SCALE, QUANTITY_SCALE } from './stockFormat'
import { WarehouseSelect } from './WarehouseSelect'

const TYPE_ICONS = { Inbound: <ImportOutlined />, Outbound: <ExportOutlined />, Transfer: <SwapOutlined /> }

interface MovementFormProps {
  /** Hareket yalnızca aktif depolarla yapılabilir. */
  warehouses: Warehouse[]
  onSaved: () => void
}

export function MovementForm({ warehouses, onSaved }: MovementFormProps) {
  const { language, t } = useLanguage()
  const { message } = App.useApp()
  const [form] = Form.useForm<MovementFormValues>()
  const [type, setType] = useState<MovementType>('Inbound')
  const [product, setProduct] = useState<Product>()
  const [saving, setSaving] = useState(false)
  const [savedCount, setSavedCount] = useState(0)

  const sourceField = sourceWarehouseField(type)
  const productId = Form.useWatch('productId', form)
  const sourceWarehouseId = Form.useWatch('sourceWarehouseId', form)
  const watchedSource = Form.useWatch(sourceField ?? 'warehouseId', form)
  const stockWarehouseId = sourceField ? watchedSource : undefined

  // Çıkış ve transferde kaynak depodaki mevcut stok, kullanıcı miktarı girerken görebilsin diye gösterilir.
  const available = useApiQuery(
    (signal) =>
      productId && stockWarehouseId
        ? stockService
            .listBalances({ productId, warehouseId: stockWarehouseId, includeEmpty: true, pageSize: 1 }, signal)
            .then((page) => page.items[0]?.quantity ?? 0)
        : Promise.resolve(undefined),
    [productId, stockWarehouseId, savedCount],
  )

  const serverErrors = useServerErrors(form, {
    rootFields: MOVEMENT_FIELDS,
    businessRuleFields: {
      'Inventory.InsufficientStock': 'quantity',
      'Inventory.TransferWarehousesMustDiffer': 'targetWarehouseId',
      'Inventory.ProductNotFound': 'productId',
      'Inventory.ProductInactive': 'productId',
      'Inventory.WarehouseNotFound': (error) => findWarehouseField(type, form.getFieldsValue(), error.parameters[0]),
      'Inventory.WarehouseInactive': (error) => findWarehouseField(type, form.getFieldsValue(), error.parameters[0]),
    },
  })

  const unit = product?.unitOfMeasureCode

  const submit = async (values: MovementFormValues) => {
    const movement = toMovementRequest(type, values)
    serverErrors.clearFormError()
    setSaving(true)

    try {
      // Hatalar global bildirim yerine formun içinde, ilgili alanın yanında gösterilir.
      const config = { skipErrorNotification: true }
      if (movement.type === 'Transfer') await stockService.transfer(movement.request, config)
      else if (movement.type === 'Inbound') await stockService.inbound(movement.request, config)
      else await stockService.outbound(movement.request, config)

      message.success(
        t(`movement.saved.${type}`, formatQuantity(movement.request.quantity, language), unit ?? '', product?.name ?? ''),
      )
      // Ürün ve depolar art arda yapılan hareketler için korunur.
      form.resetFields(['quantity', 'documentReference', 'description'])
      setSavedCount((count) => count + 1)
      onSaved()
    } catch (error) {
      serverErrors.applyServerError(error)
    } finally {
      setSaving(false)
    }
  }

  const amountRules = (requiredMessage: string, scale: number, scaleMessage: string, allowZero: boolean): FormRule[] => [
    { required: true, message: requiredMessage },
    {
      validator: (_, value?: number | null) => {
        if (value == null) return Promise.resolve()
        if (allowZero ? value < 0 : value <= 0) {
          return Promise.reject(new Error(t(allowZero ? 'movement.form.priceNegative' : 'movement.form.quantityPositive')))
        }
        return decimalPlaces(value) > scale ? Promise.reject(new Error(scaleMessage)) : Promise.resolve()
      },
    },
  ]

  const warehouseRules: FormRule[] = [{ required: true, message: t('movement.form.warehouseRequired') }]

  if (warehouses.length === 0) {
    return (
      <Alert
        type="info"
        showIcon
        title={t('movement.noWarehouses')}
        action={<Link to="/warehouses">{t('menu.warehouses')}</Link>}
      />
    )
  }

  const decimalSeparator = language === 'tr' ? ',' : '.'

  return (
    <Form<MovementFormValues>
      form={form}
      layout="vertical"
      onFinish={submit}
      onFinishFailed={serverErrors.clearFormError}
      onValuesChange={(changed: MovementFormValues, values) => {
        // Hedef listesinde kaynak depo yer almaz; kaynak sonradan hedefle aynı seçilirse hedef temizlenir.
        if (changed.sourceWarehouseId && changed.sourceWarehouseId === values.targetWarehouseId) {
          form.setFieldValue('targetWarehouseId', undefined)
        }
      }}
    >
      <Segmented<MovementType>
        block
        value={type}
        onChange={(value) => {
          form.setFieldsValue(carryOverWarehouse(value, form.getFieldsValue(true)))
          setType(value)
          serverErrors.clearFormError()
        }}
        options={(['Inbound', 'Outbound', 'Transfer'] as const).map((value) => ({
          value,
          icon: TYPE_ICONS[value],
          label: t(`movement.type.${value}`),
        }))}
        style={{ marginBottom: 16 }}
      />

      <FormErrorAlert message={serverErrors.formError} onClose={serverErrors.clearFormError} />

      <Row gutter={16}>
        <Col xs={24}>
          <Form.Item name="productId" label={t('movement.form.product')} rules={[{ required: true, message: t('movement.form.productRequired') }]}>
            <ProductSelect
              activeOnly
              placeholder={t('movement.form.productPlaceholder')}
              onProductChange={(selected) => {
                setProduct(selected)
                // Ürünün tanımlı birim fiyatı öneri olarak gelir; kullanıcı hareket anındaki fiyatla değiştirebilir.
                if (selected) form.setFieldValue('unitPrice', selected.unitPrice)
              }}
            />
          </Form.Item>
        </Col>

        {type === 'Transfer' ? (
          <>
            <Col xs={24} md={12}>
              <Form.Item name="sourceWarehouseId" label={t('movement.form.sourceWarehouse')} rules={warehouseRules}>
                <WarehouseSelect warehouses={warehouses} placeholder={t('movement.form.warehousePlaceholder')} />
              </Form.Item>
            </Col>
            <Col xs={24} md={12}>
              <Form.Item
                name="targetWarehouseId"
                label={t('movement.form.targetWarehouse')}
                rules={warehouseRules}
              >
                <WarehouseSelect
                  warehouses={warehouses.filter((warehouse) => warehouse.id !== sourceWarehouseId)}
                  placeholder={t('movement.form.warehousePlaceholder')}
                />
              </Form.Item>
            </Col>
          </>
        ) : (
          <Col xs={24}>
            <Form.Item
              name="warehouseId"
              label={t(type === 'Inbound' ? 'movement.form.inboundWarehouse' : 'movement.form.outboundWarehouse')}
              rules={warehouseRules}
            >
              <WarehouseSelect warehouses={warehouses} placeholder={t('movement.form.warehousePlaceholder')} />
            </Form.Item>
          </Col>
        )}

        <Col xs={24} md={12}>
          <Form.Item
            name="quantity"
            label={t('movement.form.quantity')}
            extra={
              available.data !== undefined
                ? t('movement.form.available', formatQuantity(available.data, language), unit ?? '')
                : undefined
            }
            rules={amountRules(
              t('movement.form.quantityRequired'),
              QUANTITY_SCALE,
              t('movement.form.quantityScale', QUANTITY_SCALE),
              false,
            )}
          >
            <InputNumber
              min={0}
              decimalSeparator={decimalSeparator}
              suffix={unit}
              style={{ width: '100%' }}
              placeholder="0"
            />
          </Form.Item>
        </Col>
        <Col xs={24} md={12}>
          <Form.Item
            name="unitPrice"
            label={t('movement.form.unitPrice')}
            rules={amountRules(
              t('movement.form.unitPriceRequired'),
              PRICE_SCALE,
              t('movement.form.priceScale', PRICE_SCALE),
              true,
            )}
          >
            <InputNumber min={0} decimalSeparator={decimalSeparator} style={{ width: '100%' }} placeholder="0" />
          </Form.Item>
        </Col>

        <Col xs={24} md={12}>
          <Form.Item name="documentReference" label={t('movement.form.documentReference')}>
            <Input
              maxLength={MOVEMENT_LIMITS.documentReference}
              autoComplete="off"
              placeholder={t(`movement.form.documentPlaceholder.${type}`)}
            />
          </Form.Item>
        </Col>
        <Col xs={24} md={12}>
          <Form.Item name="description" label={t('movement.form.description')}>
            <Input maxLength={MOVEMENT_LIMITS.description} autoComplete="off" />
          </Form.Item>
        </Col>
      </Row>

      <Flex justify="end">
        <Button type="primary" htmlType="submit" loading={saving} icon={TYPE_ICONS[type]}>
          {t(`movement.submit.${type}`)}
        </Button>
      </Flex>
    </Form>
  )
}
