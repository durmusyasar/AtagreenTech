import { GlobalOutlined, LogoutOutlined, UserOutlined } from '@ant-design/icons'
import { Button, Dropdown, Flex, Select, Tag, Tooltip, Typography } from 'antd'
import { useAuth } from '../../core/context/useAuth'
import { useLanguage } from '../../core/context/useLanguage'
import { useTenant } from '../../core/context/useTenant'
import { LANGUAGE_LABELS, SUPPORTED_LANGUAGES, type Language } from '../../core/i18n/languages'

export function AppHeader() {
  const { claims, signOut } = useAuth()
  const { tenant } = useTenant()
  const { language, setLanguage, t } = useLanguage()

  return (
    <Flex align="center" justify="flex-end" gap={16} style={{ height: '100%' }}>
      {tenant && (
        <Tooltip title={t('header.tenant')}>
          <Tag color="green">{tenant}</Tag>
        </Tooltip>
      )}

      <Select<Language>
        aria-label={t('header.language')}
        value={language}
        onChange={setLanguage}
        suffixIcon={<GlobalOutlined />}
        options={SUPPORTED_LANGUAGES.map((code) => ({ value: code, label: LANGUAGE_LABELS[code] }))}
        style={{ width: 128 }}
      />

      <Dropdown
        trigger={['click']}
        menu={{
          items: [{ key: 'sign-out', icon: <LogoutOutlined />, label: t('header.signOut'), onClick: signOut }],
        }}
      >
        <Button type="text" icon={<UserOutlined />}>
          <Typography.Text ellipsis style={{ maxWidth: 200 }}>
            {claims?.email ?? claims?.subject ?? ''}
          </Typography.Text>
        </Button>
      </Dropdown>
    </Flex>
  )
}
