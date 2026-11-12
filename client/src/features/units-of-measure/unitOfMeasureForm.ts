import { toTranslationValues, type TranslationValues } from '../../components/translations/translationForm'
import type { UnitOfMeasure } from '../../types/unitOfMeasure'

/** Backend'deki `UnitOfMeasure` ve `UnitOfMeasureTranslation` uzunluk sınırları. */
export const UNIT_OF_MEASURE_LIMITS = { code: 16, name: 100, symbol: 20 } as const

export const UNIT_OF_MEASURE_TRANSLATION_FIELDS = ['name', 'symbol'] as const

export type UnitOfMeasureTranslationField = (typeof UNIT_OF_MEASURE_TRANSLATION_FIELDS)[number]

export interface UnitOfMeasureFormValues {
  code: string
  isActive: boolean
  /** Dil koduna göre çeviriler; ad ve sembolü boş bırakılan diller kaydedilmez. */
  translations: TranslationValues<UnitOfMeasureTranslationField>
}

export function toFormValues(unit?: UnitOfMeasure | null): UnitOfMeasureFormValues {
  return {
    code: unit?.code ?? '',
    isActive: unit?.isActive ?? true,
    translations: toTranslationValues(unit?.translations ?? []),
  }
}
