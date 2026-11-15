import { createContext } from 'react'
import { DEFAULT_TENANT_THEME } from '../theme/tenantTheme'
import type { TenantConfig } from '../../types/tenant'

export interface TenantThemeContextValue {
  config: TenantConfig
  isLoading: boolean
}

export const TenantThemeContext = createContext<TenantThemeContextValue>({
  config: DEFAULT_TENANT_THEME,
  isLoading: true,
})
