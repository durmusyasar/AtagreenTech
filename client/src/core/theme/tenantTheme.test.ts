import { describe, expect, it } from 'vitest'
import { DEFAULT_TENANT_THEME, derivePrimarySurfaces } from '@/core/theme/tenantTheme'

describe('derivePrimarySurfaces', () => {
  it('keeps the known green primary close to the previous static theme', () => {
    const surfaces = derivePrimarySurfaces('#2e7d32')
    expect(surfaces.colorPrimaryBg).toBe('#e8f5e9')
    expect(surfaces.colorPrimaryBgHover).toBe('#c8e6c9')
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
