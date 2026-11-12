import { describe, expect, it } from 'vitest'
import { mapServerFieldErrors } from './serverErrors'

describe('mapServerFieldErrors', () => {
  const translations = { sent: [{ languageCode: 'tr' }, { languageCode: 'en' }], fields: ['name', 'description'] }
  const map = (fieldErrors: Record<string, string[]>) =>
    mapServerFieldErrors(fieldErrors, ['code', 'parentCategoryId'], translations)

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

  it('treats translation paths as unmatched for forms without translations', () => {
    expect(mapServerFieldErrors({ Quantity: ['>0'], 'Translations[0].Name': ['x'] }, ['quantity'])).toEqual({
      fields: [{ name: ['quantity'], errors: ['>0'] }],
      unmatched: ['x'],
    })
  })
})
