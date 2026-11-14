import {
  collectTranslations,
  toTranslationValues,
  type TranslationValues,
} from '../../components/translations/translationForm'
import type { CreateProductRequest, Currency, Product, ProductTranslation } from '../../types/product'
import type { UnitOfMeasure } from '../../types/unitOfMeasure'

/** Backend'deki `Product`, `ProductTranslation` ve `Gtin` sınırları. */
export const PRODUCT_LIMITS = { sku: 64, barcode: 14, name: 200, description: 1000 } as const

/** Backend'deki `Product.SkuPattern`. */
export const SKU_PATTERN = /^[A-Za-z0-9._-]+$/

/** Backend'deki `ProductValidationRules.PriceScale`. */
export const UNIT_COST_SCALE = 2

export const DEFAULT_CURRENCY: Currency = 'TRY'

export const PRODUCT_TRANSLATION_FIELDS = ['name', 'description'] as const

export type ProductTranslationField = (typeof PRODUCT_TRANSLATION_FIELDS)[number]

export interface ProductFormValues {
  sku: string
  barcode?: string | null
  unitPrice: number | null
  currency: string
  categoryId?: string
  unitOfMeasureId?: string
  isActive: boolean
  /** Dil koduna göre çeviriler; ad ve açıklaması boş bırakılan diller kaydedilmez. */
  translations: TranslationValues<ProductTranslationField>
}

const GTIN_LENGTHS = [8, 12, 13, 14]

/**
 * Backend'deki `Gtin.IsValid`: EAN-8, UPC-A (12), EAN-13 veya GTIN-14 ve doğru kontrol hanesi. Sağdan başlayarak
 * haneler sırayla 3 ve 1 ile çarpılıp toplanır; kontrol hanesi toplamı 10'un katına tamamlar.
 */
export function isValidGtin(barcode: string): boolean {
  const value = barcode.trim()
  if (!GTIN_LENGTHS.includes(value.length) || !/^\d+$/.test(value)) return false

  const digits = [...value].map(Number)
  const check = digits.pop()!
  const sum = digits.reverse().reduce((total, digit, index) => total + digit * (index % 2 === 0 ? 3 : 1), 0)
  return (10 - (sum % 10)) % 10 === check
}

export function toFormValues(product?: Product | null): ProductFormValues {
  return {
    sku: product?.sku ?? '',
    barcode: product?.barcode ?? null,
    unitPrice: product?.unitPrice ?? null,
    currency: product?.currency ?? DEFAULT_CURRENCY,
    categoryId: product?.categoryId,
    unitOfMeasureId: product?.unitOfMeasureId,
    isActive: product?.isActive ?? true,
    translations: toTranslationValues(product?.translations ?? []),
  }
}

/** Formun doğruladığı değerlerden istek gövdesi; boş barkod ve açıklamalar için `null` gönderilir. */
export function toProductRequest(values: ProductFormValues, languages: readonly string[]): CreateProductRequest {
  return {
    sku: values.sku.trim(),
    barcode: values.barcode?.trim() || null,
    unitPrice: values.unitPrice ?? 0,
    currency: values.currency,
    categoryId: values.categoryId ?? '',
    unitOfMeasureId: values.unitOfMeasureId ?? '',
    translations: collectTranslations(values.translations, languages, PRODUCT_TRANSLATION_FIELDS).map(
      (translation): ProductTranslation => ({ ...translation, description: translation.description || null }),
    ),
  }
}

export interface UnitOption {
  value: string
  label: string
  code: string
  isActive: boolean
}

/**
 * Ölçü birimi seçenekleri: aktif birimler ve ürünün mevcut birimi (pasife alınmış olsa bile, düzenlemede seçili
 * görünmesi için). Etiket arayüz dilindeki ad ve semboldür.
 */
export function unitOptions(units: readonly UnitOfMeasure[], currentId?: string): UnitOption[] {
  return units
    .filter((unit) => unit.isActive || unit.id === currentId)
    .map((unit) => ({ value: unit.id, label: `${unit.name} (${unit.symbol})`, code: unit.code, isActive: unit.isActive }))
    .sort((a, b) => a.label.localeCompare(b.label))
}
