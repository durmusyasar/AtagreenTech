import type { FormInstance, FormProps } from 'antd'
import { useState } from 'react'
import { ApiError } from '../../core/api/apiError'
import { useLanguage } from '../../core/context/useLanguage'
import {
  hasText,
  mapServerFieldErrors,
  MINIMUM_TRANSLATIONS,
  type FieldPath,
  type TranslationValues,
} from './translationForm'

interface TranslationFormOptions {
  languages: string[]
  /** Formdaki çeviri dışı alanlar (ör. `code`); sunucunun 400 hataları bu adlarla eşlenir. */
  rootFields: readonly string[]
  /** Çeviri başına alanlar (ör. `name`, `description`). */
  translationFields: readonly string[]
  /** 422 iş kuralı anahtarlarından, hatanın gösterileceği form alanına (ör. kod çakışması → `code`). */
  businessRuleFields?: Readonly<Record<string, string>>
}

/**
 * Dil sekmeli formların ortak durumu. Hatalı alanlar görünmeyen bir sekmede kalabileceği için sekme başlıkları
 * işaretlenir ve gerekirse o sekmeye geçilir; sunucu hataları ilgili alanlara, bağlanamayanlar formun üstündeki
 * uyarıya yazılır.
 */
export function useTranslationForm<Values>(
  typedForm: FormInstance<Values>,
  { languages, rootFields, translationFields, businessRuleFields = {} }: TranslationFormOptions,
) {
  const form = typedForm as FormInstance
  const { t } = useLanguage()
  const [activeLanguage, setActiveLanguage] = useState(languages[0])
  const [invalidLanguages, setInvalidLanguages] = useState<string[]>([])
  const [formError, setFormError] = useState<string>()

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

  // Sekmelerin üstündeki bir alan da hatalıysa kullanıcı onu zaten görür; sekme değiştirilmez.
  const revealFirstError = (names: FieldPath[]) => {
    const invalid = refreshInvalidLanguages()
    if (!names.some((name) => name[0] !== 'translations') && invalid.length > 0) setActiveLanguage(invalid[0])
  }

  /** Doldurulan dil sayısı yetersizse uyarıyı gösterir, boş bir dil sekmesini açar ve `false` döner. */
  const ensureMinimumTranslations = (sentCount: number, values: TranslationValues<string> | undefined): boolean => {
    if (sentCount >= MINIMUM_TRANSLATIONS) {
      setFormError(undefined)
      return true
    }
    setFormError(t('error.Localization.TranslationsBelowMinimum', MINIMUM_TRANSLATIONS))
    setActiveLanguage(languages.find((language) => !hasText(values?.[language])) ?? activeLanguage)
    return false
  }

  const applyServerError = (error: unknown, sent: readonly { languageCode: string }[]) => {
    // 401'de oturum kapanır ve kullanıcı oturum açma sayfasına yönlendirilir.
    if (!(error instanceof ApiError) || error.status === 401) return

    if (error.status === 400) {
      const { fields, unmatched } = mapServerFieldErrors(error.fieldErrors, sent, rootFields, translationFields)
      form.setFields(fields)
      revealFirstError(fields.map((field) => field.name))
      if (unmatched.length > 0) setFormError(unmatched.join(' '))
      else if (fields.length === 0) setFormError(error.message)
      return
    }

    const field = error.resourceKey ? businessRuleFields[error.resourceKey] : undefined
    if (field) form.setFields([{ name: field, errors: [error.message] }])
    else setFormError(error.message)
  }

  // Bir dilde zorunluluk aynı dildeki diğer alanlara bağlı olduğundan, değişen alanın kardeşleri yeniden doğrulanır.
  // Yalnızca zaten hata gösteren alanlar doğrulanır; kullanıcı yazmaya başlarken diğer alan erkenden kırmızıya dönmez.
  const revalidateSiblings = (changed: { translations?: TranslationValues<string> }) => {
    for (const [language, values] of Object.entries(changed.translations ?? {})) {
      const changedFields = Object.keys(values ?? {})
      const siblings = translationFields
        .filter((field) => !changedFields.includes(field))
        .map((field) => ['translations', language, field])
        .filter((name) => form.getFieldError(name).length > 0)
      if (siblings.length > 0) void form.validateFields(siblings).catch(() => undefined)
    }
  }

  const formProps: Pick<FormProps<Values>, 'onFinishFailed' | 'onValuesChange' | 'onFieldsChange'> = {
    onFinishFailed: ({ errorFields }) => {
      setFormError(undefined)
      revealFirstError(errorFields.map((field) => field.name))
    },
    onValuesChange: (changed) => revalidateSiblings(changed as { translations?: TranslationValues<string> }),
    onFieldsChange: () => refreshInvalidLanguages(),
  }

  return {
    activeLanguage,
    setActiveLanguage,
    invalidLanguages,
    formError,
    clearFormError: () => setFormError(undefined),
    ensureMinimumTranslations,
    applyServerError,
    formProps,
  }
}
