import { describe, expect, it } from 'vitest'
import {
  tenantsFilterDefaults,
  tenantsFilterSchema,
} from '@modules/system/components/TenantsFilter/schema'

describe('tenantsFilterSchema', () => {
  it('accepts search string', () => {
    expect(tenantsFilterSchema.parse({ search: 'acme' })).toEqual({ search: 'acme' })
  })

  it('defaults are valid', () => {
    expect(tenantsFilterSchema.parse(tenantsFilterDefaults)).toEqual({ search: '' })
  })
})
