import type { FormRule } from 'antd'
import { toFieldPath } from '../../core/api/apiError'
import { DEFAULT_LANGUAGE, SUPPORTED_LANGUAGES } from '../../core/i18n/languages'

/** Backend'deki `TranslationSetGuard.MinimumTranslationCount`. */
export const MINIMUM_TRANSLATIONS = 2

export type FieldPath = (string | number)[]

/** Formda çeviriler dil koduna göre tutulur: `translations.en.name`. */
export type TranslationValues<F extends string> = Record<string, Partial<Record<F, string | null>> | undefined>

export type SentTranslation<F extends string> = { languageCode: string } & Record<F, string>

export interface ServerFieldError {
  name: FieldPath
  errors: string[]
}

/**
 * Formda sekme olarak gösterilecek diller: varsayılan dil önce, ardından desteklenen diller ve kayıtta bulunan diğer
 * diller. PUT isteği çeviri listesinin tamamını değiştirdiğinden, arayüzün desteklemediği bir dildeki çeviri de
 * formda yer almalıdır; aksi halde güncellemede silinirdi.
 */
export function formLanguages(existing: readonly { languageCode: string }[] = []): string[] {
  return [...new Set([DEFAULT_LANGUAGE, ...SUPPORTED_LANGUAGES, ...existing.map((t) => t.languageCode)])]
}

export function toTranslationValues<T extends { languageCode: string }>(
  translations: readonly T[],
): Record<string, Omit<T, 'languageCode'>> {
  return Object.fromEntries(translations.map(({ languageCode, ...fields }) => [languageCode, fields]))
}

export function hasText(fields: Partial<Record<string, string | null>> | undefined): boolean {
  return Object.values(fields ?? {}).some((value) => Boolean(value?.trim()))
}

/**
 * Formdaki çevirileri `languages` sırasıyla ve yalnızca en az bir alanı doldurulmuş diller için istek gövdesine
 * çevirir; değerler kırpılır, boş alanlar `''` olur.
 */
export function collectTranslations<F extends string>(
  values: TranslationValues<F> | undefined,
  languages: readonly string[],
  fields: readonly F[],
): SentTranslation<F>[] {
  return languages
    .filter((language) => hasText(values?.[language]))
    .map(
      (language) =>
        ({
          languageCode: language,
          ...Object.fromEntries(fields.map((field) => [field, values?.[language]?.[field]?.trim() ?? ''])),
        }) as SentTranslation<F>,
    )
}

/**
 * Varsayılan dilde veya aynı dilde başka bir alan doldurulmuşsa alanı zorunlu kılar. Böylece boş bırakılan diller
 * atlanır, yarım girilen diller ise tamamlatılır.
 */
export function requiredInLanguage(language: string, otherFields: readonly string[], message: string): FormRule {
  return ({ getFieldValue }) => ({
    validator: (_, value?: string) => {
      const required =
        language === DEFAULT_LANGUAGE ||
        otherFields.some((field) => Boolean((getFieldValue(['translations', language, field]) as string | null)?.trim()))
      return required && !value?.trim() ? Promise.reject(new Error(message)) : Promise.resolve()
    },
  })
}

/**
 * Sunucunun 400 yanıtındaki alan hatalarını form alanlarına bağlar. `Translations[i]` dizini gönderilen listedeki
 * sıradır; form ise çevirileri dil koduyla tuttuğundan dizin dile çevrilir. Bir alana bağlanamayan hatalar (ör. liste
 * düzeyindeki kurallar) `unmatched` olarak döner.
 *
 * @param rootFields Formdaki çeviri dışı alanlar (ör. `code`, `parentCategoryId`).
 * @param translationFields Çeviri başına alanlar (ör. `name`, `description`).
 */
export function mapServerFieldErrors(
  fieldErrors: Record<string, string[]>,
  sent: readonly { languageCode: string }[],
  rootFields: readonly string[],
  translationFields: readonly string[],
): { fields: ServerFieldError[]; unmatched: string[] } {
  const fields: ServerFieldError[] = []
  const unmatched: string[] = []

  for (const [path, errors] of Object.entries(fieldErrors)) {
    const [root, index, property, ...rest] = toFieldPath(path)
    const translation = typeof index === 'number' ? sent[index] : undefined

    if (typeof root === 'string' && rootFields.includes(root) && index === undefined) {
      fields.push({ name: [root], errors })
    } else if (
      root === 'translations' &&
      translation &&
      typeof property === 'string' &&
      translationFields.includes(property) &&
      rest.length === 0
    ) {
      fields.push({ name: ['translations', translation.languageCode, property], errors })
    } else {
      unmatched.push(...errors)
    }
  }

  return { fields, unmatched }
}
