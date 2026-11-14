import { describe, expect, it } from 'vitest'
import { decimalPlaces, formatMoney, formatQuantity } from './stockFormat'

describe('decimalPlaces', () => {
  it.each([
    [12, 0],
    [12.5, 1],
    [0.0001, 4],
    [0.00001, 5],
    [1e-7, 7],
    [1.5e-7, 8],
    [1e21, 0],
  ])('%d has %i decimal places', (value, expected) => {
    expect(decimalPlaces(value)).toBe(expected)
  })
})

describe('formatting', () => {
  it('formats quantities with up to 4 decimals in the UI language', () => {
    expect(formatQuantity(1250.5, 'tr')).toBe('1.250,5')
    expect(formatQuantity(0.12345, 'en')).toBe('0.1235')
  })

  it('formats prices with 2 decimals and the currency symbol in the UI language', () => {
    expect(formatMoney(1234.5, 'TRY', 'tr')).toBe('₺1.234,50')
    expect(formatMoney(12, 'USD', 'en')).toBe('$12.00')
    expect(formatMoney(12, 'EUR', 'tr')).toBe('€12,00')
  })
})
