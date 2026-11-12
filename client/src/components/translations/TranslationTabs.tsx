import { Badge, Space, Tabs, Typography } from 'antd'
import type { ReactNode } from 'react'
import { useLanguage } from '../../core/context/useLanguage'
import { DEFAULT_LANGUAGE, isSupportedLanguage, LANGUAGE_LABELS } from '../../core/i18n/languages'

interface TranslationTabsProps {
  languages: string[]
  activeLanguage: string
  onChange: (language: string) => void
  invalidLanguages: string[]
  /** Bir dilin form alanları; alan adları `['translations', language, alan]` biçiminde olmalıdır. */
  children: (language: string) => ReactNode
}

/**
 * Dil başına bir sekme. Tüm sekmeler baştan render edilir: aksi halde hiç açılmamış bir sekmedeki alanlar forma
 * kaydolmaz ve doğrulamaya girmez.
 */
export function TranslationTabs({ languages, activeLanguage, onChange, invalidLanguages, children }: TranslationTabsProps) {
  const { t } = useLanguage()

  return (
    <Tabs
      activeKey={activeLanguage}
      onChange={onChange}
      items={languages.map((language) => ({
        key: language,
        forceRender: true,
        label: (
          <Space size={6}>
            {invalidLanguages.includes(language) && <Badge status="error" />}
            {isSupportedLanguage(language) ? LANGUAGE_LABELS[language] : language}
            {language === DEFAULT_LANGUAGE && (
              <Typography.Text type="secondary">({t('translations.defaultLanguage')})</Typography.Text>
            )}
          </Space>
        ),
        children: children(language),
      }))}
    />
  )
}
