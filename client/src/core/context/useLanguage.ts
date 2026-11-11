import { useCallback } from 'react'
import type { Language } from '../i18n/languages'
import { translate, type MessageKey } from '../i18n/messages'
import { sessionStore } from '../session'
import { useSession } from './useSession'

export interface LanguageContextValue {
  /** Arayüz dili; listelerde kayıtların adı da bu dile göre istenir (`?language=`). */
  language: Language
  setLanguage: (language: Language) => void
  t: (key: MessageKey, ...params: unknown[]) => string
}

export function useLanguage(): LanguageContextValue {
  const { language } = useSession()
  const t = useCallback((key: MessageKey, ...params: unknown[]) => translate(language, key, params), [language])
  return { language, setLanguage: sessionStore.setLanguage, t }
}
