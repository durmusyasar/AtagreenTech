export const SUPPORTED_LANGUAGES = ['tr', 'en'] as const

export type Language = (typeof SUPPORTED_LANGUAGES)[number]

/** Backend'in `Localization:DefaultLanguage` ayarıyla aynı olmalıdır. */
export const DEFAULT_LANGUAGE: Language = 'tr'

export const LANGUAGE_LABELS: Record<Language, string> = {
  tr: 'Türkçe',
  en: 'English',
}

export function isSupportedLanguage(value: unknown): value is Language {
  return typeof value === 'string' && (SUPPORTED_LANGUAGES as readonly string[]).includes(value)
}
