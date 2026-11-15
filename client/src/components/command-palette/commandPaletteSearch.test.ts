import { describe, expect, it } from 'vitest'
import { commandPaletteShortcutLabel, filterByQuery, isCommandPaletteToggle } from './commandPaletteSearch'

describe('filterByQuery', () => {
  const items = [
    { id: 'products', searchText: 'Ürünler products sayfalar pages' },
    { id: 'new-product', searchText: 'Yeni ürün ekle new product hızlı actions' },
    { id: 'lang', searchText: 'Dili değiştir language toggle sistem system' },
  ]

  it('returns all items when the query is blank', () => {
    expect(filterByQuery(items, '  ')).toHaveLength(3)
  })

  it('matches case-insensitively across tokens', () => {
    expect(filterByQuery(items, 'ürün yeni').map((item) => item.id)).toEqual(['new-product'])
    expect(filterByQuery(items, 'PRODUCT').map((item) => item.id)).toEqual(['products', 'new-product'])
  })

  it('requires every token to match', () => {
    expect(filterByQuery(items, 'ürün depo')).toEqual([])
  })
})

describe('commandPaletteShortcutLabel', () => {
  it('uses the mac glyph on Apple platforms', () => {
    expect(commandPaletteShortcutLabel('MacIntel')).toBe('⌘K')
    expect(commandPaletteShortcutLabel('Win32')).toBe('Ctrl+K')
  })
})

describe('isCommandPaletteToggle', () => {
  it('detects Ctrl/Cmd+K without Alt', () => {
    expect(
      isCommandPaletteToggle({ ctrlKey: true, metaKey: false, altKey: false, key: 'k' } as KeyboardEvent),
    ).toBe(true)
    expect(
      isCommandPaletteToggle({ ctrlKey: false, metaKey: true, altKey: false, key: 'K' } as KeyboardEvent),
    ).toBe(true)
    expect(
      isCommandPaletteToggle({ ctrlKey: true, metaKey: false, altKey: true, key: 'k' } as KeyboardEvent),
    ).toBe(false)
  })
})
