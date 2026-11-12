import { toFieldPath } from '../../core/api/apiError'
import { DEFAULT_LANGUAGE, SUPPORTED_LANGUAGES } from '../../core/i18n/languages'
import type { UnitOfMeasure, UnitOfMeasureTranslation } from '../../types/unitOfMeasure'

/** Backend'deki `UnitOfMeasure` ve `UnitOfMeasureTranslation` uzunluk sınırları. */
export const UNIT_OF_MEASURE_LIMITS = { code: 16, name: 100, symbol: 20 } as const

/** Backend'deki `TranslationSetGuard.MinimumTranslationCount`. */
export const MINIMUM_TRANSLATIONS = 2

export interface TranslationFields {
  name?: string
  symbol?: string
}

export interface UnitOfMeasureFormValues {
  code: string
  isActive: boolean
  /** Dil koduna göre çeviriler; ad ve sembolü boş bırakılan diller kaydedilmez. */
  translations: Record<string, TranslationFields | undefined>
}

export type FieldPath = (string | number)[]

export type TranslationFieldName = ['translations', string, keyof TranslationFields]

export type ServerFieldError = { name: ['code'] | TranslationFieldName; errors: string[] }

/**
 * Formda sekme olarak gösterilecek diller: varsayılan dil önce, ardından desteklenen diller ve kayıtta bulunan diğer
 * diller. PUT isteği çeviri listesinin tamamını değiştirdiğinden, arayüzün desteklemediği bir dildeki çeviri de
 * formda yer almalıdır; aksi halde güncellemede silinirdi.
 */
export function formLanguages(unit?: UnitOfMeasure | null): string[] {
  const languages = [DEFAULT_LANGUAGE, ...SUPPORTED_LANGUAGES, ...(unit?.translations.map((t) => t.languageCode) ?? [])]
  return [...new Set(languages)]
}

export function toFormValues(unit?: UnitOfMeasure | null): UnitOfMeasureFormValues {
  return {
    code: unit?.code ?? '',
    isActive: unit?.isActive ?? true,
    translations: Object.fromEntries(
      (unit?.translations ?? []).map(({ languageCode, name, symbol }) => [languageCode, { name, symbol }]),
    ),
  }
}

export function hasText(fields: TranslationFields | undefined): boolean {
  return Boolean(fields?.name?.trim() || fields?.symbol?.trim())
}

/** Formdaki çevirileri, `languages` sırasıyla ve yalnızca doldurulmuş diller için istek gövdesine çevirir. */
export function toTranslations(values: UnitOfMeasureFormValues, languages: string[]): UnitOfMeasureTranslation[] {
  return languages
    .filter((language) => hasText(values.translations?.[language]))
    .map((language) => ({
      languageCode: language,
      name: values.translations[language]?.name?.trim() ?? '',
      symbol: values.translations[language]?.symbol?.trim() ?? '',
    }))
}

/**
 * Sunucunun 400 yanıtındaki alan hatalarını form alanlarına bağlar. `Translations[i]` dizini gönderilen listedeki
 * sıradır; form ise çevirileri dil koduyla tuttuğundan dizin dile çevrilir. Bir alana bağlanamayan hatalar (ör. liste
 * düzeyindeki kurallar) `unmatched` olarak döner.
 */
export function mapServerFieldErrors(
  fieldErrors: Record<string, string[]>,
  sent: UnitOfMeasureTranslation[],
): { fields: ServerFieldError[]; unmatched: string[] } {
  const fields: ServerFieldError[] = []
  const unmatched: string[] = []

  for (const [path, errors] of Object.entries(fieldErrors)) {
    const [root, index, property, ...rest] = toFieldPath(path)
    const translation = typeof index === 'number' ? sent[index] : undefined

    if (root === 'code' && index === undefined) {
      fields.push({ name: ['code'], errors })
    } else if (
      root === 'translations' &&
      translation &&
      (property === 'name' || property === 'symbol') &&
      rest.length === 0
    ) {
      fields.push({ name: ['translations', translation.languageCode, property], errors })
    } else {
      unmatched.push(...errors)
    }
  }

  return { fields, unmatched }
}
