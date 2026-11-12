import { App, Button, Flex, Form, Input, Modal, Switch, Typography } from 'antd'
import { useMemo, useState } from 'react'
import { FormErrorAlert } from '../../components/FormErrorAlert'
import { formLanguages, MINIMUM_TRANSLATIONS, requiredInLanguage } from '../../components/translations/translationForm'
import { TranslationTabs } from '../../components/translations/TranslationTabs'
import { useTranslationForm } from '../../components/translations/useTranslationForm'
import { useLanguage } from '../../core/context/useLanguage'
import { DEFAULT_LANGUAGE } from '../../core/i18n/languages'
import { categoryService } from '../../services/categoryService'
import type { Category } from '../../types/category'
import {
  CATEGORY_LIMITS,
  CATEGORY_TRANSLATION_FIELDS,
  parentOptions,
  toFormValues,
  toRequestTranslations,
  type CategoryFormValues,
} from './categoryForm'
import { ParentCategorySelect } from './ParentCategorySelect'

const PARENT_ERRORS = [
  'Catalog.ParentCategoryNotFound',
  'Catalog.CategoryCannotBeItsOwnParent',
  'Catalog.CategoryHierarchyCycle',
] as const

interface CategoryFormModalProps {
  open: boolean
  /** Düzenlenecek kategori; `null` ise yeni kategori oluşturulur. */
  category: Category | null
  /** Üst kategori seçeneklerinin kaynağı: tüm kategoriler. */
  categories: Category[]
  categoriesLoading: boolean
  onClose: () => void
  onSaved: () => void
}

export function CategoryFormModal({ open, category, onClose, ...formProps }: CategoryFormModalProps) {
  const { t } = useLanguage()

  return (
    <Modal
      open={open}
      title={category ? t('category.editTitle', category.code) : t('category.createTitle')}
      footer={null}
      width={680}
      maskClosable={false}
      destroyOnHidden
      onCancel={onClose}
    >
      <CategoryForm category={category} onCancel={onClose} {...formProps} />
    </Modal>
  )
}

function CategoryForm({
  category,
  categories,
  categoriesLoading,
  onCancel,
  onSaved,
}: Omit<CategoryFormModalProps, 'open' | 'onClose'> & { onCancel: () => void }) {
  const { t } = useLanguage()
  const { message } = App.useApp()
  const [form] = Form.useForm<CategoryFormValues>()
  const languages = useMemo(() => formLanguages(category?.translations), [category])
  const initialValues = useMemo(() => toFormValues(category), [category])
  const options = useMemo(() => {
    const available = parentOptions(categories, category?.id)
    const parentId = category?.parentCategoryId
    // Kategoriler henüz yüklenmediyse seçili üst kategori kimlik yerine adıyla görünsün.
    return parentId && !available.some((option) => option.value === parentId)
      ? [{ value: parentId, label: category.parentCategoryName ?? parentId, code: '', isActive: true }, ...available]
      : available
  }, [categories, category])
  const [saving, setSaving] = useState(false)
  const translationForm = useTranslationForm(form, {
    languages,
    rootFields: ['code', 'parentCategoryId'],
    translationFields: CATEGORY_TRANSLATION_FIELDS,
    businessRuleFields: {
      'Catalog.CategoryCodeAlreadyExists': 'code',
      ...Object.fromEntries(PARENT_ERRORS.map((key) => [key, 'parentCategoryId'])),
    },
  })

  const submit = async (values: CategoryFormValues) => {
    const translations = toRequestTranslations(values, languages)
    if (!translationForm.ensureMinimumTranslations(translations.length, values.translations)) return

    const code = values.code.trim()
    const parentCategoryId = values.parentCategoryId ?? null
    setSaving(true)

    try {
      // Hatalar global bildirim yerine formun içinde, ilgili alanın yanında gösterilir.
      const config = { skipErrorNotification: true }
      if (category) {
        await categoryService.update(
          category.id,
          { code, translations, parentCategoryId, isActive: values.isActive },
          config,
        )
      } else {
        await categoryService.create({ code, translations, parentCategoryId }, config)
      }
      message.success(t(category ? 'category.updated' : 'category.created', code))
      onSaved()
    } catch (error) {
      translationForm.applyServerError(error, translations)
    } finally {
      setSaving(false)
    }
  }

  return (
    <Form<CategoryFormValues>
      form={form}
      layout="vertical"
      initialValues={initialValues}
      onFinish={submit}
      {...translationForm.formProps}
    >
      <FormErrorAlert message={translationForm.formError} onClose={translationForm.clearFormError} />

      <Flex gap={16} align="start">
        <Form.Item
          name="code"
          label={t('category.form.code')}
          extra={t('category.form.codeHelp')}
          normalize={(value: string) => value.toUpperCase()}
          rules={[{ required: true, whitespace: true, message: t('category.form.codeRequired') }]}
          style={{ flex: 1 }}
        >
          <Input maxLength={CATEGORY_LIMITS.code} showCount autoFocus autoComplete="off" placeholder="TOHUM" />
        </Form.Item>

        {category && (
          <Form.Item name="isActive" label={t('form.isActive')} valuePropName="checked">
            <Switch />
          </Form.Item>
        )}
      </Flex>

      <Form.Item
        name="parentCategoryId"
        label={t('category.form.parent')}
        extra={category ? t('category.form.parentHelp') : undefined}
      >
        <ParentCategorySelect
          options={options}
          loading={categoriesLoading}
          placeholder={t('category.form.parentPlaceholder')}
        />
      </Form.Item>

      <Typography.Paragraph type="secondary" style={{ marginBottom: 4 }}>
        {t('category.form.translationsHelp', DEFAULT_LANGUAGE.toUpperCase(), MINIMUM_TRANSLATIONS)}
      </Typography.Paragraph>

      <TranslationTabs
        languages={languages}
        activeLanguage={translationForm.activeLanguage}
        onChange={translationForm.setActiveLanguage}
        invalidLanguages={translationForm.invalidLanguages}
      >
        {(language) => (
          <>
            <Form.Item
              name={['translations', language, 'name']}
              label={t('category.form.name')}
              required={language === DEFAULT_LANGUAGE}
              rules={[requiredInLanguage(language, ['description'], t('category.form.nameRequired'))]}
            >
              <Input maxLength={CATEGORY_LIMITS.name} showCount autoComplete="off" lang={language} />
            </Form.Item>
            <Form.Item name={['translations', language, 'description']} label={t('category.form.description')}>
              <Input.TextArea
                maxLength={CATEGORY_LIMITS.description}
                showCount
                autoSize={{ minRows: 2, maxRows: 6 }}
                lang={language}
              />
            </Form.Item>
          </>
        )}
      </TranslationTabs>

      <Flex justify="end" gap={8}>
        <Button onClick={onCancel}>{t('common.cancel')}</Button>
        <Button type="primary" htmlType="submit" loading={saving}>
          {t('common.save')}
        </Button>
      </Flex>
    </Form>
  )
}
