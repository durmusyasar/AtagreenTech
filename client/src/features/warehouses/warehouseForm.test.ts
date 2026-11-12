import { describe, expect, it } from 'vitest'
import { toFormValues, toRequest, WAREHOUSE_CODE_PATTERN } from './warehouseForm'

describe('warehouse form', () => {
  it('starts a new warehouse as active', () => {
    expect(toFormValues(null)).toEqual({ code: '', name: '', address: '', isActive: true })
  })

  it('trims values and sends an empty address as null', () => {
    expect(toRequest({ code: ' MTR-01 ', name: ' Merkez ', address: '  ', isActive: true })).toEqual({
      code: 'MTR-01',
      name: 'Merkez',
      address: null,
    })
  })

  it.each([
    ['MTR-01', true],
    ['DEPO_2.A', true],
    ['DEPO 1', false],
    ['İZMİR', false],
  ])('code %s matches the backend pattern: %s', (code, valid) => {
    expect(WAREHOUSE_CODE_PATTERN.test(code)).toBe(valid)
  })
})
