import type { Language } from '../../core/i18n/languages'

/** Backend'deki `StockBalance.QuantityScale`: miktarlar en fazla 4 ondalık basamak taşır (ör. 12,5 kg). */
export const QUANTITY_SCALE = 4

/** Backend'deki `MovementValidationRules.PriceScale`. */
export const PRICE_SCALE = 2

/** Bir sayının ondalık basamak sayısı; `1e-7` gibi üstel yazımlar da hesaba katılır. */
export function decimalPlaces(value: number): number {
  if (!Number.isFinite(value)) return 0
  const [mantissa, exponent = '0'] = value.toString().toLowerCase().split('e')
  const fraction = mantissa.split('.')[1]?.length ?? 0
  return Math.max(0, fraction - Number(exponent))
}

export function formatQuantity(value: number, language: Language): string {
  return new Intl.NumberFormat(language, { maximumFractionDigits: QUANTITY_SCALE }).format(value)
}

/** Para birimi simgesiyle tutar (ör. `₺12,50`, `$12.50`); `currency` ISO 4217 kodudur. */
export function formatMoney(value: number, currency: string, language: Language): string {
  return new Intl.NumberFormat(language, {
    style: 'currency',
    currency,
    minimumFractionDigits: PRICE_SCALE,
    maximumFractionDigits: PRICE_SCALE,
  }).format(value)
}

export function formatDateTime(value: string, language: Language): string {
  return new Intl.DateTimeFormat(language, { dateStyle: 'short', timeStyle: 'short' }).format(new Date(value))
}
