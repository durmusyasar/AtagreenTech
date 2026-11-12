import type { FormInstance, FormProps } from 'antd'
import { useState } from 'react'
import type { FieldPath } from '../forms/serverErrors'
import { useServerErrors, type ServerErrorOptions } from '../forms/useServerErrors'
import { useLanguage } from '../../core/context/useLanguage'
import { hasText, MINIMUM_TRANSLATIONS, type TranslationValues } from './translationForm'

interface TranslationFormOptions extends Omit<ServerErrorOptions, 'onFieldErrors'> {
  languages: string[]
  /** Çeviri başına alanlar (ör. `name`, `description`). */
  translationFields: readonly string[]
}

/**
 * Dil sekmeli formların ortak durumu. Hatalı alanlar görünmeyen bir sekmede kalabileceği için sekme başlıkları
 * işaretlenir ve gerekirse o sekmeye geçilir; sunucu hataları ilgili alanlara, bağlanamayanlar formun üstündeki
 * uyarıya yazılır.
 */
export function useTranslationForm<Values>(
  typedForm: FormInstance<Values>,
  { languages, translationFields, ...serverErrorOptions }: TranslationFormOptions,
) {
  const form = typedForm as FormInstance
  const { t } = useLanguage()
  const [activeLanguage, setActiveLanguage] = useState(languages[0])
  const [invalidLanguages, setInvalidLanguages] = useState<string[]>([])

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

  const serverErrors = useServerErrors(typedForm, {
    ...serverErrorOptions,
    onFieldErrors: (fields) => revealFirstError(fields.map((field) => field.name)),
  })

  /** Doldurulan dil sayısı yetersizse uyarıyı gösterir, boş bir dil sekmesini açar ve `false` döner. */
  const ensureMinimumTranslations = (sentCount: number, values: TranslationValues<string> | undefined): boolean => {
    if (sentCount >= MINIMUM_TRANSLATIONS) {
      serverErrors.clearFormError()
      return true
    }
    serverErrors.setFormError(t('error.Localization.TranslationsBelowMinimum', MINIMUM_TRANSLATIONS))
    setActiveLanguage(languages.find((language) => !hasText(values?.[language])) ?? activeLanguage)
    return false
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
      serverErrors.clearFormError()
      revealFirstError(errorFields.map((field) => field.name))
    },
    onValuesChange: (changed) => revalidateSiblings(changed as { translations?: TranslationValues<string> }),
    onFieldsChange: () => refreshInvalidLanguages(),
  }

  return {
    activeLanguage,
    setActiveLanguage,
    invalidLanguages,
    formError: serverErrors.formError,
    clearFormError: serverErrors.clearFormError,
    ensureMinimumTranslations,
    applyServerError: (error: unknown, sent: readonly { languageCode: string }[]) =>
      serverErrors.applyServerError(error, { sent, fields: translationFields }),
    formProps,
  }
}
