import { Card, Flex, Select, Typography } from 'antd'
import type { ReactNode } from 'react'
import { BrandMark } from '../../components/BrandMark'
import { useLanguage } from '../../core/context/useLanguage'
import { LANGUAGE_LABELS, SUPPORTED_LANGUAGES, type Language } from '../../core/i18n/languages'

interface AuthPageShellProps {
  title: string
  description?: string
  children: ReactNode
}

/** Public auth ekranları için ortak kart + marka + dil seçici. */
export function AuthPageShell({ title, description, children }: AuthPageShellProps) {
  const { language, setLanguage, t } = useLanguage()

  return (
    <Flex align="center" justify="center" style={{ minHeight: '100vh', padding: 24 }}>
      <Card style={{ width: '100%', maxWidth: 480 }}>
        <Flex align="flex-start" justify="space-between" gap={12} style={{ marginBottom: 24 }}>
          <Flex vertical gap={8} style={{ minWidth: 0 }}>
            <BrandMark size={32} nameStyle={{ fontSize: 20, margin: 0 }} />
            <Typography.Title level={4} style={{ margin: 0 }}>
              {title}
            </Typography.Title>
            {description ? <Typography.Text type="secondary">{description}</Typography.Text> : null}
          </Flex>
          <Select<Language>
            size="small"
            aria-label={t('header.language')}
            value={language}
            onChange={setLanguage}
            options={SUPPORTED_LANGUAGES.map((code) => ({ value: code, label: LANGUAGE_LABELS[code] }))}
            style={{ width: 120, flexShrink: 0 }}
          />
        </Flex>
        {children}
      </Card>
    </Flex>
  )
}
