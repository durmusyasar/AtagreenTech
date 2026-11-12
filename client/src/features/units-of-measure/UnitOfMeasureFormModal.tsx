import {
  Alert,
  App,
  Badge,
  Button,
  Flex,
  Form,
  Input,
  Modal,
  Space,
  Switch,
  Tabs,
  Typography,
  type FormRule,
} from 'antd'
import { useMemo, useState } from 'react'
import { ApiError } from '../../core/api/apiError'
import { useLanguage } from '../../core/context/useLanguage'
import { DEFAULT_LANGUAGE, isSupportedLanguage, LANGUAGE_LABELS } from '../../core/i18n/languages'
import { unitOfMeasureService } from '../../services/unitOfMeasureService'
import type { UnitOfMeasure, UnitOfMeasureTranslation } from '../../types/unitOfMeasure'
import {
  formLanguages,
  hasText,
  mapServerFieldErrors,
  MINIMUM_TRANSLATIONS,
  toFormValues,
  toTranslations,
  UNIT_OF_MEASURE_LIMITS,
  type FieldPath,
  type TranslationFieldName,
  type UnitOfMeasureFormValues,
} from './unitOfMeasureForm'

interface UnitOfMeasureFormModalProps {
  open: boolean
  /** Düzenlenecek birim; `null` ise yeni birim oluşturulur. */
  unit: UnitOfMeasure | null
  onClose: () => void
  onSaved: () => void
}

export function UnitOfMeasureFormModal({ open, unit, onClose, onSaved }: UnitOfMeasureFormModalProps) {
  const { t } = useLanguage()

  return (
    <Modal
      open={open}
      title={unit ? t('uom.editTitle', unit.code) : t('uom.createTitle')}
      footer={null}
      width={600}
      maskClosable={false}
      destroyOnHidden
      onCancel={onClose}
    >
      <UnitOfMeasureForm unit={unit} onCancel={onClose} onSaved={onSaved} />
    </Modal>
  )
}

function UnitOfMeasureForm({ unit, onCancel, onSaved }: Omit<UnitOfMeasureFormModalProps, 'open' | 'onClose'> & {
  onCancel: () => void
}) {
  const { t } = useLanguage()
  const { message } = App.useApp()
  const [form] = Form.useForm<UnitOfMeasureFormValues>()
  const languages = useMemo(() => formLanguages(unit), [unit])
  const initialValues = useMemo(() => toFormValues(unit), [unit])

  const [activeLanguage, setActiveLanguage] = useState(languages[0])
  const [invalidLanguages, setInvalidLanguages] = useState<string[]>([])
  const [formError, setFormError] = useState<string>()
  const [saving, setSaving] = useState(false)

  // Hatalı alanlar başka bir sekmede kalabilir; sekme başlıkları işaretlenir ve gerekirse o sekmeye geçilir.
  const refreshInvalidLanguages = (): string[] => {
    const invalid = new Set(
      form
        .getFieldsError()
        .filter((field) => field.errors.length > 0 && field.name[0] === 'translations')
        .map((field) => String(field.name[1])),
    )
    const ordered = languages.filter((language) => invalid.has(language))
    setInvalidLanguages(ordered)
    return ordered
  }

  const revealFirstError = (fields: FieldPath[]) => {
    const invalid = refreshInvalidLanguages()
    const codeInvalid = fields.some((name) => name[0] === 'code')
    if (!codeInvalid && invalid.length > 0) setActiveLanguage(invalid[0])
  }

  const submit = async (values: UnitOfMeasureFormValues) => {
    const translations = toTranslations(values, languages)

    if (translations.length < MINIMUM_TRANSLATIONS) {
      setFormError(t('error.Localization.TranslationsBelowMinimum', MINIMUM_TRANSLATIONS))
      setActiveLanguage(languages.find((language) => !hasText(values.translations?.[language])) ?? activeLanguage)
      return
    }

    const code = values.code.trim()
    setFormError(undefined)
    setSaving(true)

    try {
      // Hatalar global bildirim yerine formun içinde, ilgili alanın yanında gösterilir.
      const config = { skipErrorNotification: true }
      if (unit) {
        await unitOfMeasureService.update(unit.id, { code, translations, isActive: values.isActive }, config)
      } else {
        await unitOfMeasureService.create({ code, translations }, config)
      }
      message.success(t(unit ? 'uom.updated' : 'uom.created', code))
      onSaved()
    } catch (error) {
      if (error instanceof ApiError) showServerError(error, translations)
    } finally {
      setSaving(false)
    }
  }

  const showServerError = (error: ApiError, sent: UnitOfMeasureTranslation[]) => {
    // 401'de oturum kapanır ve kullanıcı oturum açma sayfasına yönlendirilir.
    if (error.status === 401) return

    if (error.status === 400) {
      const { fields, unmatched } = mapServerFieldErrors(error.fieldErrors, sent)
      form.setFields(fields)
      revealFirstError(fields.map((field) => field.name))
      if (unmatched.length > 0) setFormError(unmatched.join(' '))
      else if (fields.length === 0) setFormError(error.message)
      return
    }

    if (error.resourceKey === 'Catalog.UnitOfMeasureCodeAlreadyExists') {
      form.setFields([{ name: 'code', errors: [error.message] }])
      return
    }

    setFormError(error.message)
  }

  // Bir dilde ad veya sembolden biri girildiyse diğeri de zorunludur. Kardeş alan yalnızca zaten hata gösteriyorsa
  // yeniden doğrulanır; böylece kullanıcı yazmaya başlarken diğer alan erkenden kırmızıya dönmez.
  const revalidateSibling = (changed: Partial<UnitOfMeasureFormValues>) => {
    for (const [language, fields] of Object.entries(changed.translations ?? {})) {
      for (const field of Object.keys(fields ?? {})) {
        const sibling: TranslationFieldName = ['translations', language, field === 'name' ? 'symbol' : 'name']
        if (form.getFieldError(sibling).length > 0) void form.validateFields([sibling]).catch(() => undefined)
      }
    }
  }

  const translationRules = (language: string, field: 'name' | 'symbol', sibling: 'name' | 'symbol'): FormRule[] => [
    ({ getFieldValue }) => ({
      validator: (_, value?: string) => {
        const required =
          language === DEFAULT_LANGUAGE || Boolean((getFieldValue(['translations', language, sibling]) as string)?.trim())
        return required && !value?.trim()
          ? Promise.reject(new Error(t(field === 'name' ? 'uom.form.nameRequired' : 'uom.form.symbolRequired')))
          : Promise.resolve()
      },
    }),
  ]

  return (
    <Form<UnitOfMeasureFormValues>
      form={form}
      layout="vertical"
      initialValues={initialValues}
      onFinish={submit}
      onFinishFailed={({ errorFields }) => {
        setFormError(undefined)
        revealFirstError(errorFields.map((field) => field.name))
      }}
      onValuesChange={revalidateSibling}
      onFieldsChange={() => refreshInvalidLanguages()}
    >
      {formError && (
        <Alert
          type="error"
          showIcon
          closable
          title={formError}
          onClose={() => setFormError(undefined)}
          style={{ marginBottom: 16 }}
        />
      )}

      <Flex gap={16} align="start">
        <Form.Item
          name="code"
          label={t('uom.form.code')}
          extra={t('uom.form.codeHelp')}
          normalize={(value: string) => value.toUpperCase()}
          rules={[{ required: true, whitespace: true, message: t('uom.form.codeRequired') }]}
          style={{ flex: 1 }}
        >
          <Input maxLength={UNIT_OF_MEASURE_LIMITS.code} showCount autoFocus autoComplete="off" placeholder="KG" />
        </Form.Item>

        {unit && (
          <Form.Item name="isActive" label={t('uom.form.isActive')} valuePropName="checked">
            <Switch />
          </Form.Item>
        )}
      </Flex>

      <Typography.Paragraph type="secondary" style={{ marginBottom: 4 }}>
        {t('uom.form.translationsHelp', DEFAULT_LANGUAGE.toUpperCase(), MINIMUM_TRANSLATIONS)}
      </Typography.Paragraph>

      <Tabs
        activeKey={activeLanguage}
        onChange={setActiveLanguage}
        items={languages.map((language) => ({
          key: language,
          forceRender: true,
          label: (
            <Space size={6}>
              {invalidLanguages.includes(language) && <Badge status="error" />}
              {isSupportedLanguage(language) ? LANGUAGE_LABELS[language] : language}
              {language === DEFAULT_LANGUAGE && (
                <Typography.Text type="secondary">({t('uom.form.defaultLanguage')})</Typography.Text>
              )}
            </Space>
          ),
          children: (
            <Flex gap={16}>
              <Form.Item
                name={['translations', language, 'name']}
                label={t('uom.form.name')}
                required={language === DEFAULT_LANGUAGE}
                rules={translationRules(language, 'name', 'symbol')}
                style={{ flex: 2 }}
              >
                <Input maxLength={UNIT_OF_MEASURE_LIMITS.name} autoComplete="off" lang={language} />
              </Form.Item>
              <Form.Item
                name={['translations', language, 'symbol']}
                label={t('uom.form.symbol')}
                required={language === DEFAULT_LANGUAGE}
                rules={translationRules(language, 'symbol', 'name')}
                style={{ flex: 1 }}
              >
                <Input maxLength={UNIT_OF_MEASURE_LIMITS.symbol} autoComplete="off" lang={language} />
              </Form.Item>
            </Flex>
          ),
        }))}
      />

      <Flex justify="end" gap={8}>
        <Button onClick={onCancel}>
          {t('common.cancel')}
        </Button>
        <Button type="primary" htmlType="submit" loading={saving}>
          {t('common.save')}
        </Button>
      </Flex>
    </Form>
  )
}
