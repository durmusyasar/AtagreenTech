export const SUPPORTED_LANGUAGES = ['tr', 'en'] as const

export type Language = (typeof SUPPORTED_LANGUAGES)[number]

/** Backend'in `Localization:DefaultLanguage` ayarıyla aynı olmalıdır. */
export const DEFAULT_LANGUAGE: Language = 'tr'

/** react-i18next / LanguageDetector localStorage anahtarı (Issue #109). */
export const I18N_LANGUAGE_STORAGE_KEY = 'omnisync_lang'

export const LANGUAGE_LABELS: Record<Language, string> = {
  tr: 'Türkçe',
  en: 'English',
}

export function isSupportedLanguage(value: unknown): value is Language {
  return typeof value === 'string' && (SUPPORTED_LANGUAGES as readonly string[]).includes(value)
}
