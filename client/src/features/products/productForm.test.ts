import { describe, expect, it } from 'vitest'
import type { Product } from '../../types/product'
import type { UnitOfMeasure } from '../../types/unitOfMeasure'
import { isValidGtin, toFormValues, toProductRequest, unitOptions } from './productForm'

describe('isValidGtin', () => {
  it.each(['96385074', '036000291452', '4006381333931', '10614141000415', ' 4006381333931 '])('accepts %s', (barcode) => {
    expect(isValidGtin(barcode)).toBe(true)
  })

  it.each(['', '4006381333932', '1234567', '400638133393', '400638133393A', '4006381 33931'])('rejects %s', (barcode) => {
    expect(isValidGtin(barcode)).toBe(false)
  })
})

const product: Product = {
  id: 'p1',
  sku: 'DSB-16MM',
  barcode: '4006381333931',
  name: 'Damla Sulama Borusu',
  description: null,
  unitPrice: 12.5,
  currency: 'USD',
  categoryId: 'c1',
  categoryName: 'Sulama',
  unitOfMeasureId: 'u1',
  unitOfMeasureCode: 'M',
  isActive: false,
  createdAt: '2026-11-14T00:00:00Z',
  translations: [
    { languageCode: 'tr', name: 'Damla Sulama Borusu', description: null },
    { languageCode: 'en', name: 'Drip Pipe', description: '16 mm' },
  ],
}

describe('toFormValues', () => {
  it('defaults a new product to TRY and active', () => {
    expect(toFormValues(null)).toMatchObject({ sku: '', barcode: null, unitPrice: null, currency: 'TRY', isActive: true })
  })

  it('keys translations of an existing product by language', () => {
    expect(toFormValues(product)).toMatchObject({
      currency: 'USD',
      isActive: false,
      translations: { tr: { name: 'Damla Sulama Borusu', description: null }, en: { name: 'Drip Pipe', description: '16 mm' } },
    })
  })
})

describe('toProductRequest', () => {
  it('trims values, sends blank barcode and descriptions as null and skips empty languages', () => {
    const request = toProductRequest(
      {
        sku: ' dsb-16mm ',
        barcode: '  ',
        unitPrice: 12.5,
        currency: 'EUR',
        categoryId: 'c1',
        unitOfMeasureId: 'u1',
        isActive: true,
        translations: { tr: { name: ' Boru ', description: ' ' }, en: { name: 'Pipe', description: 'Pipe' }, de: {} },
      },
      ['tr', 'en', 'de'],
    )

    expect(request).toEqual({
      sku: 'dsb-16mm',
      barcode: null,
      unitPrice: 12.5,
      currency: 'EUR',
      categoryId: 'c1',
      unitOfMeasureId: 'u1',
      translations: [
        { languageCode: 'tr', name: 'Boru', description: null },
        { languageCode: 'en', name: 'Pipe', description: 'Pipe' },
      ],
    })
  })
})

describe('unitOptions', () => {
  const unit = (id: string, name: string, isActive: boolean): UnitOfMeasure => ({
    id,
    code: id.toUpperCase(),
    name,
    symbol: id,
    isActive,
    translations: [],
  })

  it('lists active units and keeps the current inactive one, sorted by label', () => {
    const units = [unit('m', 'Metre', true), unit('kg', 'Kilogram', true), unit('lt', 'Litre', false), unit('pc', 'Adet', false)]

    expect(unitOptions(units, 'lt').map((option) => option.value)).toEqual(['kg', 'lt', 'm'])
  })
})
