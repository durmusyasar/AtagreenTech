import { describe, expect, it } from 'vitest'
import { sortOrderForField, toListPageSorter } from '@/Components/ListPage/sorter'

describe('toListPageSorter', () => {
  it('maps single sorter', () => {
    expect(toListPageSorter({ field: 'name', order: 'ascend' })).toEqual({
      field: 'name',
      order: 'ascend',
    })
  })

  it('returns undefined when order cleared', () => {
    expect(toListPageSorter({ field: 'name', order: undefined })).toBeUndefined()
  })

  it('uses columnKey when field missing', () => {
    expect(toListPageSorter({ columnKey: 'slug', order: 'descend' })).toEqual({
      field: 'slug',
      order: 'descend',
    })
  })
})

describe('sortOrderForField', () => {
  it('matches field', () => {
    expect(sortOrderForField({ field: 'name', order: 'ascend' }, 'name')).toBe('ascend')
    expect(sortOrderForField({ field: 'name', order: 'ascend' }, 'slug')).toBeUndefined()
  })
})
