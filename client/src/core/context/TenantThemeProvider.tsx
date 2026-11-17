import { ConfigProvider, theme as antdTheme } from 'antd'
import enUS from 'antd/locale/en_US'
import trTR from 'antd/locale/tr_TR'
import { useEffect, type ReactNode } from 'react'
import { tenantConfigQuery } from '../../services/tenantService'
import { useThemeStore } from '../../stores/themeStore'
import type { TenantConfig } from '../../types/tenant'
import { useAppQuery } from '../hooks/useAppQuery'
import type { Language } from '../i18n/languages'
import {
  applyCssPrimary,
  applyDocumentTitle,
  applyFavicon,
  DEFAULT_TENANT_THEME,
  derivePrimarySurfaces,
} from '../theme/tenantTheme'
import { TenantThemeContext } from './tenantThemeContext'
import { useLanguage } from './useLanguage'

const ANTD_LOCALES = { tr: trTR, en: enUS } satisfies Record<Language, unknown>

/**
 * Aktif tenant'ın marka ayarlarını çeker; CSS değişkenleri, favicon, belge başlığı ve Ant Design
 * `colorPrimary` değerlerini uygular. Login dahil tüm ağaç bu sağlayıcının altında olmalıdır.
 */
export function TenantThemeProvider({ children }: { children: ReactNode }) {
  const { language } = useLanguage()
  const resolvedTheme = useThemeStore((state) => state.resolvedTheme)
  const setTheme = useThemeStore((state) => state.setTheme)
  const themeMode = useThemeStore((state) => state.theme)
  const query = useAppQuery({
    ...tenantConfigQuery,
    staleTime: 5 * 60_000,
    retry: 1,
  })

  const config: TenantConfig = query.data
    ? {
        tenantId: query.data.tenantId,
        subdomain: query.data.subdomain,
        name: query.data.name || DEFAULT_TENANT_THEME.name,
        primaryColor: query.data.primaryColor || DEFAULT_TENANT_THEME.primaryColor,
        logoUrl: query.data.logoUrl || DEFAULT_TENANT_THEME.logoUrl,
        faviconUrl: query.data.faviconUrl || DEFAULT_TENANT_THEME.faviconUrl,
      }
    : DEFAULT_TENANT_THEME

  useEffect(() => {
    applyCssPrimary(config.primaryColor)
    if (config.faviconUrl) applyFavicon(config.faviconUrl)
    applyDocumentTitle(config.name)
  }, [config.primaryColor, config.faviconUrl, config.name])

  // Persist rehydrate / FOUC sonrası document sınıfı ile store'u hizala.
  useEffect(() => {
    setTheme(themeMode)
  }, [setTheme, themeMode])

  const surfaces = derivePrimarySurfaces(config.primaryColor)

  return (
    <TenantThemeContext.Provider value={{ config, isLoading: query.isLoading }}>
      <ConfigProvider
        locale={ANTD_LOCALES[language]}
        theme={{
          algorithm: resolvedTheme === 'dark' ? antdTheme.darkAlgorithm : antdTheme.defaultAlgorithm,
          token: {
            colorPrimary: config.primaryColor,
            colorPrimaryBg: surfaces.colorPrimaryBg,
            colorPrimaryBgHover: surfaces.colorPrimaryBgHover,
          },
        }}
      >
        {children}
      </ConfigProvider>
    </TenantThemeContext.Provider>
  )
}
