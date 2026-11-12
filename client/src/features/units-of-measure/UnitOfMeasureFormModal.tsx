import { App, Button, Flex, Form, Input, Modal, Switch, Typography } from 'antd'
import { useMemo, useState } from 'react'
import { FormErrorAlert } from '../../components/FormErrorAlert'
import {
  collectTranslations,
  formLanguages,
  MINIMUM_TRANSLATIONS,
  requiredInLanguage,
} from '../../components/translations/translationForm'
import { TranslationTabs } from '../../components/translations/TranslationTabs'
import { useTranslationForm } from '../../components/translations/useTranslationForm'
import { useLanguage } from '../../core/context/useLanguage'
import { DEFAULT_LANGUAGE } from '../../core/i18n/languages'
import { unitOfMeasureService } from '../../services/unitOfMeasureService'
import type { UnitOfMeasure } from '../../types/unitOfMeasure'
import {
  toFormValues,
  UNIT_OF_MEASURE_LIMITS,
  UNIT_OF_MEASURE_TRANSLATION_FIELDS,
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
  const languages = useMemo(() => formLanguages(unit?.translations), [unit])
  const initialValues = useMemo(() => toFormValues(unit), [unit])
  const [saving, setSaving] = useState(false)
  const translationForm = useTranslationForm(form, {
    languages,
    rootFields: ['code'],
    translationFields: UNIT_OF_MEASURE_TRANSLATION_FIELDS,
    businessRuleFields: { 'Catalog.UnitOfMeasureCodeAlreadyExists': 'code' },
  })

  const submit = async (values: UnitOfMeasureFormValues) => {
    const translations = collectTranslations(values.translations, languages, UNIT_OF_MEASURE_TRANSLATION_FIELDS)
    if (!translationForm.ensureMinimumTranslations(translations.length, values.translations)) return

    const code = values.code.trim()
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
      translationForm.applyServerError(error, translations)
    } finally {
      setSaving(false)
    }
  }

  return (
    <Form<UnitOfMeasureFormValues>
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
          label={t('uom.form.code')}
          extra={t('uom.form.codeHelp')}
          normalize={(value: string) => value.toUpperCase()}
          rules={[{ required: true, whitespace: true, message: t('uom.form.codeRequired') }]}
          style={{ flex: 1 }}
        >
          <Input maxLength={UNIT_OF_MEASURE_LIMITS.code} showCount autoFocus autoComplete="off" placeholder="KG" />
        </Form.Item>

        {unit && (
          <Form.Item name="isActive" label={t('form.isActive')} valuePropName="checked">
            <Switch />
          </Form.Item>
        )}
      </Flex>

      <Typography.Paragraph type="secondary" style={{ marginBottom: 4 }}>
        {t('uom.form.translationsHelp', DEFAULT_LANGUAGE.toUpperCase(), MINIMUM_TRANSLATIONS)}
      </Typography.Paragraph>

      <TranslationTabs
        languages={languages}
        activeLanguage={translationForm.activeLanguage}
        onChange={translationForm.setActiveLanguage}
        invalidLanguages={translationForm.invalidLanguages}
      >
        {(language) => (
          <Flex gap={16}>
            <Form.Item
              name={['translations', language, 'name']}
              label={t('uom.form.name')}
              required={language === DEFAULT_LANGUAGE}
              rules={[requiredInLanguage(language, ['symbol'], t('uom.form.nameRequired'))]}
              style={{ flex: 2 }}
            >
              <Input maxLength={UNIT_OF_MEASURE_LIMITS.name} autoComplete="off" lang={language} />
            </Form.Item>
            <Form.Item
              name={['translations', language, 'symbol']}
              label={t('uom.form.symbol')}
              required={language === DEFAULT_LANGUAGE}
              rules={[requiredInLanguage(language, ['name'], t('uom.form.symbolRequired'))]}
              style={{ flex: 1 }}
            >
              <Input maxLength={UNIT_OF_MEASURE_LIMITS.symbol} autoComplete="off" lang={language} />
            </Form.Item>
          </Flex>
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
