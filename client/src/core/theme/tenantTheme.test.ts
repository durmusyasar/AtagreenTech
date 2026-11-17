import { describe, expect, it } from 'vitest'
import { DEFAULT_TENANT_THEME, derivePrimarySurfaces } from '@/core/theme/tenantTheme'

describe('derivePrimarySurfaces', () => {
  it('keeps the default Atagreen primary on brand-soft surfaces', () => {
    const surfaces = derivePrimarySurfaces(DEFAULT_TENANT_THEME.primaryColor)
    expect(surfaces.colorPrimaryBg).toBe('#dcfce7')
    expect(surfaces.colorPrimaryBgHover).toBe('#bbf7d0')
  })

  it('falls back when the hex is invalid', () => {
    expect(derivePrimarySurfaces('not-a-color')).toEqual(derivePrimarySurfaces(DEFAULT_TENANT_THEME.primaryColor))
  })

  it('derives lighter surfaces for a custom primary', () => {
    const surfaces = derivePrimarySurfaces('#1565c0')
    expect(surfaces.colorPrimaryBg).not.toBe('#1565c0')
    expect(surfaces.colorPrimaryBg.startsWith('#')).toBe(true)
    expect(surfaces.colorPrimaryBgHover).not.toBe(surfaces.colorPrimaryBg)
  })
})
