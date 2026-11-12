import { describe, expect, it } from 'vitest'
import type { UnitOfMeasure } from '../../types/unitOfMeasure'
import { formLanguages, mapServerFieldErrors, toFormValues, toTranslations } from './unitOfMeasureForm'

const unit: UnitOfMeasure = {
  id: 'u-1',
  code: 'KG',
  name: 'Kilogram',
  symbol: 'kg',
  isActive: false,
  translations: [
    { languageCode: 'de', name: 'Kilogramm', symbol: 'kg' },
    { languageCode: 'tr', name: 'Kilogram', symbol: 'kg' },
  ],
}

describe('formLanguages', () => {
  it('lists the default language first, then supported languages', () => {
    expect(formLanguages(null)).toEqual(['tr', 'en'])
  })

  it('keeps languages the UI does not offer so an update does not delete them', () => {
    expect(formLanguages(unit)).toEqual(['tr', 'en', 'de'])
  })
})

describe('toFormValues', () => {
  it('starts a new unit as active with no translations', () => {
    expect(toFormValues(null)).toEqual({ code: '', isActive: true, translations: {} })
  })

  it('keys translations by language', () => {
    expect(toFormValues(unit)).toEqual({
      code: 'KG',
      isActive: false,
      translations: { de: { name: 'Kilogramm', symbol: 'kg' }, tr: { name: 'Kilogram', symbol: 'kg' } },
    })
  })
})

describe('toTranslations', () => {
  it('sends filled languages in tab order, trimmed', () => {
    const values = {
      code: 'LT',
      isActive: true,
      translations: { en: { name: ' Litre ', symbol: 'l ' }, tr: { name: 'Litre', symbol: 'lt' }, de: {} },
    }

    expect(toTranslations(values, ['tr', 'en', 'de'])).toEqual([
      { languageCode: 'tr', name: 'Litre', symbol: 'lt' },
      { languageCode: 'en', name: 'Litre', symbol: 'l' },
    ])
  })

  it('skips languages with only whitespace', () => {
    const values = { code: 'LT', isActive: true, translations: { tr: { name: 'Litre', symbol: 'lt' }, en: { name: '  ' } } }
    expect(toTranslations(values, ['tr', 'en']).map((t) => t.languageCode)).toEqual(['tr'])
  })
})

describe('mapServerFieldErrors', () => {
  const sent = [
    { languageCode: 'tr', name: 'Litre', symbol: 'lt' },
    { languageCode: 'en', name: '', symbol: 'l' },
  ]

  it('maps translation indexes back to the language tabs', () => {
    const result = mapServerFieldErrors(
      { Code: ['Birim kodu boş olamaz.'], 'Translations[1].Name': ['Ölçü birimi adı boş olamaz.'] },
      sent,
    )

    expect(result).toEqual({
      fields: [
        { name: ['code'], errors: ['Birim kodu boş olamaz.'] },
        { name: ['translations', 'en', 'name'], errors: ['Ölçü birimi adı boş olamaz.'] },
      ],
      unmatched: [],
    })
  })

  it('accepts camelCase paths', () => {
    expect(mapServerFieldErrors({ 'translations[0].symbol': ['x'] }, sent).fields).toEqual([
      { name: ['translations', 'tr', 'symbol'], errors: ['x'] },
    ])
  })

  it('returns list-level and unknown errors as unmatched', () => {
    const result = mapServerFieldErrors(
      {
        Translations: ['Aynı dil için birden fazla çeviri girilemez.'],
        'Translations[0].LanguageCode': ['Dil kodu geçersiz.'],
        'Translations[5].Name': ['out of range'],
      },
      sent,
    )

    expect(result.fields).toEqual([])
    expect(result.unmatched).toEqual(['Aynı dil için birden fazla çeviri girilemez.', 'Dil kodu geçersiz.', 'out of range'])
  })
})
