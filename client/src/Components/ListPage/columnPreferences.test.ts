import { describe, expect, it } from 'vitest'
import {
  applyColumnPreferences,
  buildDefaultColumnPreferences,
  clampColumnWidth,
  mergeColumnPreferences,
  resolveColumnKey,
} from '@/Components/ListPage/columnPreferences'
import type { ListTableColumnsType } from '@/Components/ListPage/type'

type Row = { name: string; slug: string }

const columns: ListTableColumnsType<Row> = [
  { title: 'Ad', dataIndex: 'name', width: 160 },
  { title: 'Slug', dataIndex: 'slug', key: 'slug', width: 120 },
  { title: 'Aksiyon', key: 'actions', listColumnLocked: true, width: 100 },
]

describe('columnPreferences', () => {
  it('resolves key from dataIndex or key', () => {
    expect(resolveColumnKey(columns[0]!)).toBe('name')
    expect(resolveColumnKey(columns[2]!)).toBe('actions')
  })

  it('builds defaults from columns', () => {
    expect(buildDefaultColumnPreferences(columns)).toEqual([
      { key: 'name', visible: true, width: 160 },
      { key: 'slug', visible: true, width: 120 },
      { key: 'actions', visible: true, width: 100 },
    ])
  })

  it('merges stored order and drops unknown keys', () => {
    const defaults = buildDefaultColumnPreferences(columns)
    const merged = mergeColumnPreferences(defaults, {
      columns: [
        { key: 'slug', visible: false, width: 200 },
        { key: 'ghost', visible: true },
        { key: 'name', visible: true },
      ],
    })
    expect(merged.map((item) => item.key)).toEqual(['slug', 'name', 'actions'])
    expect(merged[0]).toEqual({ key: 'slug', visible: false, width: 200 })
  })

  it('applies visibility order and width', () => {
    const prefs = [
      { key: 'slug', visible: true, width: 220 },
      { key: 'name', visible: false },
      { key: 'actions', visible: true, width: 100 },
    ]
    const applied = applyColumnPreferences(columns, prefs)
    expect(applied).toHaveLength(2)
    expect(resolveColumnKey(applied[0]!)).toBe('slug')
    expect(applied[0]?.width).toBe(220)
  })

  it('clamps width bounds', () => {
    expect(clampColumnWidth(10)).toBe(80)
    expect(clampColumnWidth(9999)).toBe(800)
    expect(clampColumnWidth(undefined)).toBeUndefined()
  })
})
