import { Card, Flex, Typography } from 'antd'
import { BrandMark } from '../../../components/BrandMark'
import { LanguageSwitcher } from '../../../components/LanguageSwitcher'
import { ThemeSwitcher } from '../../../components/ThemeSwitcher'
import type { AuthPageShellProps } from './type'

/** Public auth ekranları için ortak kart + marka + tema / dil seçici. */
export function AuthPageShell({ title, description, children }: AuthPageShellProps) {
  return (
    <Flex
      align="center"
      justify="center"
      className="min-h-screen bg-slate-50 text-slate-900 dark:bg-slate-950 dark:text-slate-100"
      style={{ padding: 24 }}
    >
      <Card
        className="border-slate-200 bg-white shadow-sm dark:border-slate-800 dark:bg-slate-900"
        style={{ width: '100%', maxWidth: 480 }}
      >
        <Flex align="flex-start" justify="space-between" gap={12} style={{ marginBottom: 24 }}>
          <Flex vertical gap={8} style={{ minWidth: 0 }}>
            <BrandMark size={32} nameStyle={{ fontSize: 20, margin: 0 }} />
            <Typography.Title level={4} style={{ margin: 0 }}>
              {title}
            </Typography.Title>
            {description ? <Typography.Text type="secondary">{description}</Typography.Text> : null}
          </Flex>
          <Flex align="center" gap={2} style={{ flexShrink: 0 }}>
            <ThemeSwitcher variant="toggle" />
            <LanguageSwitcher size="small" />
          </Flex>
        </Flex>
        {children}
      </Card>
    </Flex>
  )
}
