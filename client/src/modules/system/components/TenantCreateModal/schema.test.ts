import { describe, expect, it } from 'vitest'
import { tenantCreateSchema } from '@modules/system/components/TenantCreateModal/schema'

describe('tenantCreateSchema', () => {
  it('accepts valid name, slug and optional email', () => {
    const result = tenantCreateSchema.safeParse({
      name: 'Acme',
      slug: 'acme',
      adminEmail: 'admin@acme.com',
    })
    expect(result.success).toBe(true)
  })

  it('accepts empty admin email', () => {
    const result = tenantCreateSchema.safeParse({
      name: 'Acme',
      slug: 'acme',
      adminEmail: '',
    })
    expect(result.success).toBe(true)
  })

  it('rejects empty name/slug with i18n keys', () => {
    const result = tenantCreateSchema.safeParse({
      name: '  ',
      slug: '',
      adminEmail: '',
    })
    expect(result.success).toBe(false)
    if (result.success) return
    const messages = result.error.issues.map((issue) => issue.message)
    expect(messages).toContain('tenants.form.nameRequired')
    expect(messages).toContain('tenants.form.slugRequired')
  })

  it('rejects invalid slug', () => {
    const result = tenantCreateSchema.safeParse({
      name: 'Acme',
      slug: 'WWW',
      adminEmail: '',
    })
    expect(result.success).toBe(false)
    if (result.success) return
    expect(
      result.error.issues.some((issue) => issue.message === 'tenants.form.slugInvalid'),
    ).toBe(true)
  })

  it('rejects invalid admin email', () => {
    const result = tenantCreateSchema.safeParse({
      name: 'Acme',
      slug: 'acme',
      adminEmail: 'not-an-email',
    })
    expect(result.success).toBe(false)
    if (result.success) return
    expect(
      result.error.issues.some((issue) => issue.message === 'tenants.form.adminEmailInvalid'),
    ).toBe(true)
  })
})
