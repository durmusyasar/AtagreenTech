import { describe, expect, it } from 'vitest'
import { collectTranslations, formLanguages, hasText, toTranslationValues } from './translationForm'

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
