import { useCallback, useEffect } from 'react'
import { useTranslation } from 'react-i18next'
import type { Language } from '../i18n/languages'
import { isSupportedLanguage } from '../i18n/languages'
import { translate, type MessageKey } from '../i18n/messages'
import { sessionStore } from '../session'
import { useSession } from './useSession'

export interface LanguageContextValue {
  /** Arayüz dili; listelerde kayıtların adı da bu dile göre istenir (`?language=`). */
  language: Language
  setLanguage: (language: Language) => void
  t: (key: MessageKey, ...params: unknown[]) => string
}

/**
 * Dil + `t()` erişimi. Kaynak react-i18next; tercih sessionStore / `omnisync_lang` ile senkron.
 */
export function useLanguage(): LanguageContextValue {
  const { language } = useSession()
  const { i18n } = useTranslation()

  useEffect(() => {
    if (i18n.resolvedLanguage !== language) {
      void i18n.changeLanguage(language)
    }
  }, [i18n, language])

  const setLanguage = useCallback(
    (next: Language) => {
      if (!isSupportedLanguage(next)) return
      sessionStore.setLanguage(next)
      void i18n.changeLanguage(next)
    },
    [i18n],
  )

  const t = useCallback(
    (key: MessageKey, ...params: unknown[]) => translate(language, key, params),
    [language],
  )

  return { language, setLanguage, t }
}
