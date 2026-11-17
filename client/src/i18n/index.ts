import i18n from 'i18next'
import LanguageDetector from 'i18next-browser-languagedetector'
import { initReactI18next } from 'react-i18next'
import { DEFAULT_LANGUAGE, I18N_LANGUAGE_STORAGE_KEY, SUPPORTED_LANGUAGES } from '@/core/i18n/languages'

import enAuth from '@/i18n/locales/en/auth.json'
import enCommon from '@/i18n/locales/en/common.json'
import enRbac from '@/i18n/locales/en/rbac.json'
import enSecurity from '@/i18n/locales/en/security.json'
import enUsers from '@/i18n/locales/en/users.json'
import trAuth from '@/i18n/locales/tr/auth.json'
import trCommon from '@/i18n/locales/tr/common.json'
import trRbac from '@/i18n/locales/tr/rbac.json'
import trSecurity from '@/i18n/locales/tr/security.json'
import trUsers from '@/i18n/locales/tr/users.json'

export { I18N_LANGUAGE_STORAGE_KEY }

export const defaultNS = 'common' as const
export const namespaces = ['common', 'auth', 'users', 'rbac', 'security'] as const

export const resources = {
  tr: {
    common: trCommon,
    auth: trAuth,
    users: trUsers,
    rbac: trRbac,
    security: trSecurity,
  },
  en: {
    common: enCommon,
    auth: enAuth,
    users: enUsers,
    rbac: enRbac,
    security: enSecurity,
  },
} as const

void i18n
  .use(LanguageDetector)
  .use(initReactI18next)
  .init({
    resources,
    fallbackLng: DEFAULT_LANGUAGE,
    supportedLngs: [...SUPPORTED_LANGUAGES],
    defaultNS,
    ns: [...namespaces],
    // Düz anahtarlar: `login.title`, `errors.AUTH_INVALID_CREDENTIALS`
    keySeparator: false,
    nsSeparator: ':',
    detection: {
      order: ['localStorage', 'navigator'],
      lookupLocalStorage: I18N_LANGUAGE_STORAGE_KEY,
      caches: ['localStorage'],
      // tr-* → tr; diğer tüm navigator dilleri → en
      convertDetectedLanguage: (lng: string) => (lng.toLowerCase().startsWith('tr') ? 'tr' : 'en'),
    },
    interpolation: {
      escapeValue: false,
    },
  })

export default i18n
