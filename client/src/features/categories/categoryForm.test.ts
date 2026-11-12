import { describe, expect, it } from 'vitest'
import type { Category } from '../../types/category'
import { parentOptions, selfAndDescendantIds, toFormValues, toRequestTranslations } from './categoryForm'

const category = (id: string, name: string, parentCategoryId: string | null = null, isActive = true): Category => ({
  id,
  code: id.toUpperCase(),
  name,
  description: null,
  parentCategoryId,
  parentCategoryName: null,
  isActive,
  createdAt: '2026-11-11T00:00:00Z',
  translations: [{ languageCode: 'tr', name, description: null }],
})

// sulama ─┬─ damla ── boru
//         └─ yagmur
// tohum
const categories = [
  category('boru', 'Boru', 'damla'),
  category('damla', 'Damla', 'sulama'),
  category('sulama', 'Sulama'),
  category('tohum', 'Tohum', null, false),
  category('yagmur', 'Yağmurlama', 'sulama'),
]

describe('selfAndDescendantIds', () => {
  it('collects the whole subtree', () => {
    expect([...selfAndDescendantIds(categories, 'sulama')].sort()).toEqual(['boru', 'damla', 'sulama', 'yagmur'])
    expect([...selfAndDescendantIds(categories, 'boru')]).toEqual(['boru'])
  })

  it('terminates on cyclic data', () => {
    const cyclic = [category('a', 'A', 'b'), category('b', 'B', 'a')]
    expect([...selfAndDescendantIds(cyclic, 'a')].sort()).toEqual(['a', 'b'])
  })
})

describe('parentOptions', () => {
  it('labels options with their path from the root, sorted', () => {
    expect(parentOptions(categories).map((option) => option.label)).toEqual([
      'Sulama',
      'Sulama / Damla',
      'Sulama / Damla / Boru',
      'Sulama / Yağmurlama',
      'Tohum',
    ])
  })

  it('excludes the edited category and its descendants to prevent cycles', () => {
    expect(parentOptions(categories, 'damla').map((option) => option.value)).toEqual(['sulama', 'yagmur', 'tohum'])
  })

  it('keeps inactive categories selectable and marks them', () => {
    expect(parentOptions(categories).find((option) => option.value === 'tohum')).toEqual({
      value: 'tohum',
      label: 'Tohum',
      code: 'TOHUM',
      isActive: false,
    })
  })
})

describe('toFormValues', () => {
  it('starts a new category as an active top-level category', () => {
    expect(toFormValues(null)).toEqual({ code: '', isActive: true, parentCategoryId: null, translations: {} })
  })

  it('keeps the parent and keys translations by language', () => {
    expect(toFormValues(categories[1])).toEqual({
      code: 'DAMLA',
      isActive: true,
      parentCategoryId: 'sulama',
      translations: { tr: { name: 'Damla', description: null } },
    })
  })
})

describe('toRequestTranslations', () => {
  it('sends an empty description as null', () => {
    const values = {
      code: 'TOHUM',
      isActive: true,
      translations: { tr: { name: ' Tohum ', description: '  ' }, en: { name: 'Seed', description: ' Seeds ' } },
    }

    expect(toRequestTranslations(values, ['tr', 'en'])).toEqual([
      { languageCode: 'tr', name: 'Tohum', description: null },
      { languageCode: 'en', name: 'Seed', description: 'Seeds' },
    ])
  })
})
