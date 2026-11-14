import { useMutation, useQueryClient } from '@tanstack/react-query'
import {
  App,
  Button,
  Col,
  Flex,
  Form,
  Input,
  InputNumber,
  Modal,
  Row,
  Select,
  Space,
  Switch,
  Tag,
  Typography,
  type FormRule,
} from 'antd'
import { useMemo } from 'react'
import { FormErrorAlert } from '../../components/forms/FormErrorAlert'
import { formLanguages, MINIMUM_TRANSLATIONS, requiredInLanguage } from '../../components/translations/translationForm'
import { TranslationTabs } from '../../components/translations/TranslationTabs'
import { useTranslationForm } from '../../components/translations/useTranslationForm'
import { invalidate } from '../../core/api/queryClient'
import { useLanguage } from '../../core/context/useLanguage'
import { DEFAULT_LANGUAGE } from '../../core/i18n/languages'
import { productService } from '../../services/productService'
import { queryKeys } from '../../services/queryKeys'
import type { Category } from '../../types/category'
import { CURRENCIES, type CreateProductRequest, type Product } from '../../types/product'
import type { UnitOfMeasure } from '../../types/unitOfMeasure'
import { parentOptions } from '../categories/categoryForm'
import { ParentCategorySelect } from '../categories/ParentCategorySelect'
import { decimalPlaces } from '../stock/stockFormat'
import {
  isValidGtin,
  PRODUCT_LIMITS,
  PRODUCT_TRANSLATION_FIELDS,
  SKU_PATTERN,
  toFormValues,
  toProductRequest,
  UNIT_COST_SCALE,
  unitOptions,
  type ProductFormValues,
} from './productForm'

const ROOT_FIELDS = ['sku', 'barcode', 'unitPrice', 'currency', 'categoryId', 'unitOfMeasureId'] as const

interface ProductFormModalProps {
  open: boolean
  /** Düzenlenecek ürün; `null` ise yeni ürün oluşturulur. */
  product: Product | null
  /** Kategori seçeneklerinin kaynağı: tüm kategoriler. */
  categories: Category[]
  categoriesLoading: boolean
  /** Ölçü birimi seçeneklerinin kaynağı: tüm birimler. */
  units: UnitOfMeasure[]
  unitsLoading: boolean
  onClose: () => void
  onSaved: () => void
}

export function ProductFormModal({ open, product, onClose, ...formProps }: ProductFormModalProps) {
  const { t } = useLanguage()

  return (
    <Modal
      open={open}
      title={product ? t('product.editTitle', product.sku) : t('product.createTitle')}
      footer={null}
      width={760}
      maskClosable={false}
      destroyOnHidden
      onCancel={onClose}
    >
      <ProductForm product={product} onCancel={onClose} {...formProps} />
    </Modal>
  )
}

function ProductForm({
  product,
  categories,
  categoriesLoading,
  units,
  unitsLoading,
  onCancel,
  onSaved,
}: Omit<ProductFormModalProps, 'open' | 'onClose'> & { onCancel: () => void }) {
  const { language, t } = useLanguage()
  const { message } = App.useApp()
  const [form] = Form.useForm<ProductFormValues>()
  const languages = useMemo(() => formLanguages(product?.translations), [product])
  const initialValues = useMemo(() => toFormValues(product), [product])

  // Pasif kategori ve birimler yeni seçimde sunulmaz; ürünün mevcut kategorisi veya birimi pasifse yine de görünür.
  const categoryOptions = useMemo(() => {
    const available = parentOptions(categories).filter((option) => option.isActive || option.value === product?.categoryId)
    return product && !available.some((option) => option.value === product.categoryId)
      ? [{ value: product.categoryId, label: product.categoryName, code: '', isActive: true }, ...available]
      : available
  }, [categories, product])
  const unitSelectOptions = useMemo(() => {
    const available = unitOptions(units, product?.unitOfMeasureId)
    return product && !available.some((option) => option.value === product.unitOfMeasureId)
      ? [
          { value: product.unitOfMeasureId, label: product.unitOfMeasureCode, code: product.unitOfMeasureCode, isActive: true },
          ...available,
        ]
      : available
  }, [units, product])

  const queryClient = useQueryClient()
  const save = useMutation({
    // Hatalar global bildirim yerine formun içinde, ilgili alanın yanında gösterilir.
    mutationFn: ({ request, isActive }: { request: CreateProductRequest; isActive: boolean }): Promise<unknown> =>
      product
        ? productService.update(product.id, { ...request, isActive }, { skipErrorNotification: true })
        : productService.create(request, { skipErrorNotification: true }),
    // Stok hareketleri ve bakiyeler ürün adını ve para birimini gösterir.
    onSuccess: () => invalidate(queryClient, queryKeys.products, queryKeys.stock),
  })
  const translationForm = useTranslationForm(form, {
    languages,
    rootFields: ROOT_FIELDS,
    translationFields: PRODUCT_TRANSLATION_FIELDS,
    businessRuleFields: {
      'Catalog.SkuAlreadyExists': 'sku',
      'Catalog.BarcodeAlreadyExists': 'barcode',
      'Catalog.ProductCurrencyLocked': 'currency',
      'Catalog.CategoryNotFound': 'categoryId',
      'Catalog.UnitOfMeasureNotFound': 'unitOfMeasureId',
    },
  })

  const submit = async (values: ProductFormValues) => {
    const request = toProductRequest(values, languages)
    if (!translationForm.ensureMinimumTranslations(request.translations.length, values.translations)) return

    try {
      await save.mutateAsync({ request, isActive: values.isActive })
      message.success(t(product ? 'product.updated' : 'product.created', request.sku.toUpperCase()))
      onSaved()
    } catch (error) {
      translationForm.applyServerError(error, request.translations)
    }
  }

  const barcodeRules: FormRule[] = [
    {
      validator: (_, value?: string | null) =>
        !value?.trim() || isValidGtin(value) ? Promise.resolve() : Promise.reject(new Error(t('product.form.barcodeInvalid'))),
    },
  ]
  const unitCostRules: FormRule[] = [
    { required: true, message: t('product.form.unitCostRequired') },
    {
      validator: (_, value?: number | null) => {
        if (value == null) return Promise.resolve()
        if (value <= 0) return Promise.reject(new Error(t('product.form.unitCostPositive')))
        return decimalPlaces(value) > UNIT_COST_SCALE
          ? Promise.reject(new Error(t('product.form.unitCostScale', UNIT_COST_SCALE)))
          : Promise.resolve()
      },
    },
  ]

  return (
    <Form<ProductFormValues>
      form={form}
      layout="vertical"
      initialValues={initialValues}
      onFinish={submit}
      {...translationForm.formProps}
    >
      <FormErrorAlert message={translationForm.formError} onClose={translationForm.clearFormError} />

      <Row gutter={16}>
        <Col xs={24} md={product ? 10 : 12}>
          <Form.Item
            name="sku"
            label={t('product.form.sku')}
            extra={t('product.form.skuHelp')}
            normalize={(value: string) => value.toUpperCase()}
            rules={[
              { required: true, whitespace: true, message: t('product.form.skuRequired') },
              { pattern: SKU_PATTERN, message: t('product.form.skuPattern') },
            ]}
          >
            <Input maxLength={PRODUCT_LIMITS.sku} autoFocus autoComplete="off" placeholder="DSB-16MM" />
          </Form.Item>
        </Col>
        <Col xs={24} md={product ? 10 : 12}>
          <Form.Item
            name="barcode"
            label={t('product.form.barcode')}
            extra={t('product.form.barcodeHelp')}
            rules={barcodeRules}
          >
            <Input
              maxLength={PRODUCT_LIMITS.barcode}
              inputMode="numeric"
              autoComplete="off"
              allowClear
              placeholder="8690000000000"
            />
          </Form.Item>
        </Col>
        {product && (
          <Col xs={24} md={4}>
            <Form.Item name="isActive" label={t('form.isActive')} valuePropName="checked">
              <Switch />
            </Form.Item>
          </Col>
        )}

        <Col xs={24} md={12}>
          <Form.Item
            name="categoryId"
            label={t('product.form.category')}
            rules={[{ required: true, message: t('product.form.categoryRequired') }]}
          >
            <ParentCategorySelect
              options={categoryOptions}
              loading={categoriesLoading}
              placeholder={t('product.form.categoryPlaceholder')}
            />
          </Form.Item>
        </Col>
        <Col xs={24} md={12}>
          <Form.Item
            name="unitOfMeasureId"
            label={t('product.form.unitOfMeasure')}
            rules={[{ required: true, message: t('product.form.unitOfMeasureRequired') }]}
          >
            <Select
              showSearch={{ optionFilterProp: ['label', 'code'] }}
              options={unitSelectOptions}
              loading={unitsLoading}
              placeholder={t('product.form.unitOfMeasurePlaceholder')}
              optionRender={({ data }) => (
                <Flex justify="space-between" align="center" gap={8}>
                  <Typography.Text ellipsis>{data.label}</Typography.Text>
                  <span>
                    {!data.isActive && <Tag>{t('list.inactive')}</Tag>}
                    <Typography.Text type="secondary" code>
                      {data.code}
                    </Typography.Text>
                  </span>
                </Flex>
              )}
            />
          </Form.Item>
        </Col>

        <Col xs={24} md={12}>
          {/* İçteki noStyle alanların (birim maliyet, para birimi) hataları bu Form.Item'ın altında gösterilir. */}
          <Form.Item
            label={t('product.form.unitCost')}
            required
            extra={t(product ? 'product.form.currencyLockedHelp' : 'product.form.currencyHelp')}
          >
            <Space.Compact block>
              <Form.Item name="unitPrice" noStyle rules={unitCostRules}>
                <InputNumber
                  min={0}
                  decimalSeparator={language === 'tr' ? ',' : '.'}
                  style={{ width: '100%' }}
                  placeholder="0"
                  aria-label={t('product.form.unitCost')}
                />
              </Form.Item>
              <Form.Item name="currency" noStyle rules={[{ required: true, message: t('product.form.currencyRequired') }]}>
                <Select
                  aria-label={t('product.form.currency')}
                  options={CURRENCIES.map((currency) => ({ value: currency, label: currency }))}
                  style={{ width: 96 }}
                />
              </Form.Item>
            </Space.Compact>
          </Form.Item>
        </Col>
      </Row>

      <Typography.Paragraph type="secondary" style={{ marginBottom: 4 }}>
        {t('product.form.translationsHelp', DEFAULT_LANGUAGE.toUpperCase(), MINIMUM_TRANSLATIONS)}
      </Typography.Paragraph>

      <TranslationTabs
        languages={languages}
        activeLanguage={translationForm.activeLanguage}
        onChange={translationForm.setActiveLanguage}
        invalidLanguages={translationForm.invalidLanguages}
      >
        {(code) => (
          <>
            <Form.Item
              name={['translations', code, 'name']}
              label={t('product.form.name')}
              required={code === DEFAULT_LANGUAGE}
              rules={[requiredInLanguage(code, ['description'], t('product.form.nameRequired'))]}
            >
              <Input maxLength={PRODUCT_LIMITS.name} showCount autoComplete="off" lang={code} />
            </Form.Item>
            <Form.Item name={['translations', code, 'description']} label={t('product.form.description')}>
              <Input.TextArea
                maxLength={PRODUCT_LIMITS.description}
                showCount
                autoSize={{ minRows: 2, maxRows: 6 }}
                lang={code}
              />
            </Form.Item>
          </>
        )}
      </TranslationTabs>

      <Flex justify="end" gap={8}>
        <Button onClick={onCancel}>{t('common.cancel')}</Button>
        <Button type="primary" htmlType="submit" loading={save.isPending}>
          {t('common.save')}
        </Button>
      </Flex>
    </Form>
  )
}
