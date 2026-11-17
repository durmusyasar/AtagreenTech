import type { ReactNode } from 'react'

export interface TranslationTabsProps {
  languages: string[]
  activeLanguage: string
  onChange: (language: string) => void
  invalidLanguages: string[]
  /** Bir dilin form alanları; alan adları `['translations', language, alan]` biçiminde olmalıdır. */
  children: (language: string) => ReactNode
}
