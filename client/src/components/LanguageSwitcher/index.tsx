import { GlobalOutlined } from '@ant-design/icons'
import { Segmented } from 'antd'
import { useTranslation } from 'react-i18next'
import { useLanguage } from '../../core/context/useLanguage'
import type { Language } from '../../core/i18n/languages'
import type { LanguageSwitcherProps } from './type'

/**
 * TR/EN dil seçici. Tercih `omnisync_lang` (i18next) + sessionStore üzerinden kalıcıdır.
 */
export function LanguageSwitcher({ className, size = 'middle' }: LanguageSwitcherProps) {
  const { i18n } = useTranslation()
  const { language, setLanguage } = useLanguage()
  const current = (i18n.resolvedLanguage === 'en' || language === 'en' ? 'en' : 'tr') as Language

  return (
    <span className={className} style={{ display: 'inline-flex', alignItems: 'center', gap: 6 }}>
      <GlobalOutlined aria-hidden />
      <Segmented<Language>
        size={size}
        value={current}
        aria-label="Language switcher"
        options={[
          { label: 'TR', value: 'tr' },
          { label: 'EN', value: 'en' },
        ]}
        onChange={(lang) => {
          if (lang !== current) setLanguage(lang)
        }}
      />
    </span>
  )
}
