import type { NavigationTranslations } from '@/Components/Sidebar/type'

/** Dil değişiminde ekstra round-trip olmadan başlık seçer. */
export function resolveNavTitle(
  translations: NavigationTranslations | undefined,
  fallbackTitle: string,
  language: string,
): string {
  if (!translations) return fallbackTitle
  const localized = translations[language]
  if (localized && localized.trim()) return localized
  return fallbackTitle
}
