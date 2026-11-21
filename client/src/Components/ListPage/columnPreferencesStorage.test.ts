/** @vitest-environment happy-dom */
import { afterEach, describe, expect, it } from 'vitest'
import {
  clearColumnPreferences,
  readColumnPreferences,
  writeColumnPreferences,
} from '@/Components/ListPage/columnPreferencesStorage'

describe('columnPreferencesStorage', () => {
  const key = 'test/columns'

  afterEach(() => {
    clearColumnPreferences(key)
  })

  it('round-trips preferences', () => {
    writeColumnPreferences(key, {
      columns: [{ key: 'name', visible: true, width: 160 }],
    })
    expect(readColumnPreferences(key)).toEqual({
      columns: [{ key: 'name', visible: true, width: 160 }],
    })
  })

  it('clears preferences', () => {
    writeColumnPreferences(key, { columns: [{ key: 'name', visible: false }] })
    clearColumnPreferences(key)
    expect(readColumnPreferences(key)).toBeNull()
  })
})
