import { useContext } from 'react'
import { TenantThemeContext } from './tenantThemeContext'

/** Aktif tenant markası (logo, renk, ad) ve yükleme durumu. */
export function useTenantTheme() {
  return useContext(TenantThemeContext)
}
