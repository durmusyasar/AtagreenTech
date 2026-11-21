/** @vitest-environment happy-dom */
import { afterEach, describe, expect, it } from 'vitest'
import {
  clearListPageState,
  readListPageState,
  writeListPageState,
} from '@/Components/ListPage/listPageStorage'

describe('listPageStorage', () => {
  const key = 'test/list'

  afterEach(() => {
    clearListPageState(key)
  })

  it('returns null when nothing stored', () => {
    expect(readListPageState(key)).toBeNull()
  })

  it('round-trips filter and meta', () => {
    writeListPageState(key, {
      filter: { search: 'acme' },
      meta: { page: 2, pageSize: 20, sorter: { field: 'name', order: 'ascend' } },
    })
    expect(readListPageState(key)).toEqual({
      filter: { search: 'acme' },
      meta: { page: 2, pageSize: 20, sorter: { field: 'name', order: 'ascend' } },
    })
  })

  it('clears stored state', () => {
    writeListPageState(key, {
      filter: { search: 'x' },
      meta: { page: 1, pageSize: 20 },
    })
    clearListPageState(key)
    expect(readListPageState(key)).toBeNull()
  })
})
