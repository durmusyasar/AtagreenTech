import { describe, expect, it } from 'vitest'
import { collectTranslations, formLanguages, hasText, mapServerFieldErrors, toTranslationValues } from './translationForm'

describe('formLanguages', () => {
  it('lists the default language first, then supported languages', () => {
    expect(formLanguages()).toEqual(['tr', 'en'])
  })

  it('keeps languages the UI does not offer so an update does not delete them', () => {
    expect(formLanguages([{ languageCode: 'de' }, { languageCode: 'tr' }])).toEqual(['tr', 'en', 'de'])
  })
})

describe('toTranslationValues', () => {
  it('keys translations by language', () => {
    expect(
      toTranslationValues([
        { languageCode: 'tr', name: 'Tohum', description: null },
        { languageCode: 'en', name: 'Seed', description: 'Seeds' },
      ]),
    ).toEqual({ tr: { name: 'Tohum', description: null }, en: { name: 'Seed', description: 'Seeds' } })
  })
})

describe('hasText', () => {
  it('ignores blank and missing fields', () => {
    expect(hasText(undefined)).toBe(false)
    expect(hasText({ name: '  ', description: null })).toBe(false)
    expect(hasText({ name: '', description: 'x' })).toBe(true)
  })
})

describe('collectTranslations', () => {
  it('sends filled languages in tab order, trimmed', () => {
    const values = { en: { name: ' Litre ', symbol: 'l ' }, tr: { name: 'Litre', symbol: 'lt' }, de: {} }

    expect(collectTranslations(values, ['tr', 'en', 'de'], ['name', 'symbol'])).toEqual([
      { languageCode: 'tr', name: 'Litre', symbol: 'lt' },
      { languageCode: 'en', name: 'Litre', symbol: 'l' },
    ])
  })

  it('skips languages with only whitespace and fills missing fields with empty strings', () => {
    const values = { tr: { name: 'Tohum' }, en: { name: '  ', description: null } }
    expect(collectTranslations(values, ['tr', 'en'], ['name', 'description'])).toEqual([
      { languageCode: 'tr', name: 'Tohum', description: '' },
    ])
  })
})

describe('mapServerFieldErrors', () => {
  const sent = [{ languageCode: 'tr' }, { languageCode: 'en' }]
  const map = (fieldErrors: Record<string, string[]>) =>
    mapServerFieldErrors(fieldErrors, sent, ['code', 'parentCategoryId'], ['name', 'description'])

  it('maps root fields and translation indexes back to the language tabs', () => {
    expect(
      map({
        Code: ['Kod boş olamaz.'],
        ParentCategoryId: ['Geçersiz.'],
        'Translations[1].Description': ['Çok uzun.'],
      }),
    ).toEqual({
      fields: [
        { name: ['code'], errors: ['Kod boş olamaz.'] },
        { name: ['parentCategoryId'], errors: ['Geçersiz.'] },
        { name: ['translations', 'en', 'description'], errors: ['Çok uzun.'] },
      ],
      unmatched: [],
    })
  })

  it('accepts camelCase paths', () => {
    expect(map({ 'translations[0].name': ['x'] }).fields).toEqual([{ name: ['translations', 'tr', 'name'], errors: ['x'] }])
  })

  it('returns list-level and unknown errors as unmatched', () => {
    const result = map({
      Translations: ['Aynı dil için birden fazla çeviri girilemez.'],
      'Translations[0].LanguageCode': ['Dil kodu geçersiz.'],
      'Translations[5].Name': ['out of range'],
      IsActive: ['unknown'],
    })

    expect(result.fields).toEqual([])
    expect(result.unmatched).toEqual([
      'Aynı dil için birden fazla çeviri girilemez.',
      'Dil kodu geçersiz.',
      'out of range',
      'unknown',
    ])
  })
})
