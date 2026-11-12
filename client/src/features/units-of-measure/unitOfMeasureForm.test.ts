import { describe, expect, it } from 'vitest'
import type { UnitOfMeasure } from '../../types/unitOfMeasure'
import { toFormValues } from './unitOfMeasureForm'

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
