import i18n from 'i18next'
import LanguageDetector from 'i18next-browser-languagedetector'
import { initReactI18next } from 'react-i18next'
import { DEFAULT_LANGUAGE, I18N_LANGUAGE_STORAGE_KEY, SUPPORTED_LANGUAGES } from '@/core/i18n/languages'

import enCommon from '@/i18n/locales/en/common.json'
import enEnum from '@/i18n/locales/en/enum.json'
import enMain from '@/i18n/locales/en/main.json'
import trCommon from '@/i18n/locales/tr/common.json'
import trEnum from '@/i18n/locales/tr/enum.json'
import trMain from '@/i18n/locales/tr/main.json'

export { I18N_LANGUAGE_STORAGE_KEY }

/** Varsayılan: ekran metinleri (`login.title`, `users.title`…). */
export const defaultNS = 'main' as const
export const namespaces = ['main', 'common', 'enum'] as const

export const resources = {
  tr: {
    main: trMain,
    common: trCommon,
    enum: trEnum,
  },
  en: {
    main: enMain,
    common: enCommon,
    enum: enEnum,
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
    // Düz anahtarlar: `login.title`, `errors.AUTH_INVALID_CREDENTIALS`, `menu.dashboard`
    keySeparator: false,
    nsSeparator: ':',
    detection: {
      order: ['localStorage', 'navigator'],
      lookupLocalStorage: I18N_LANGUAGE_STORAGE_KEY,
      caches: ['localStorage'],
      convertDetectedLanguage: (lng: string) => (lng.toLowerCase().startsWith('tr') ? 'tr' : 'en'),
    },
    interpolation: {
      escapeValue: false,
    },
  })

export default i18n
