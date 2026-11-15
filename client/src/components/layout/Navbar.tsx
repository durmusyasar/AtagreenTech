import { GlobalOutlined, LogoutOutlined, UserOutlined } from '@ant-design/icons'
import { Button, Dropdown, Flex, Select, Tag, Tooltip, Typography } from 'antd'
import { Link } from 'react-router-dom'
import { useAuth } from '../../core/context/useAuth'
import { useLanguage } from '../../core/context/useLanguage'
import { useTenant } from '../../core/context/useTenant'
import { LANGUAGE_LABELS, SUPPORTED_LANGUAGES, type Language } from '../../core/i18n/languages'

/**
 * Üst çubuk: marka, aktif tenant, dil seçici (TR/EN) ve kullanıcı / çıkış.
 * Modül linkleri sol menüdedir (`AppLayout` sider); burada yalnızca oturum ve bağlam kontrolleri vardır.
 */
export function Navbar() {
  const { claims, signOut } = useAuth()
  const { tenant } = useTenant()
  const { language, setLanguage, t } = useLanguage()

  return (
    <Flex align="center" justify="space-between" gap={16} style={{ height: '100%', width: '100%' }}>
      <Flex align="center" gap={12} style={{ minWidth: 0 }}>
        <Link to="/" style={{ display: 'flex', alignItems: 'center', gap: 10, color: 'inherit' }}>
          <img src="/favicon.svg" alt="" width={24} height={24} />
          <Typography.Text strong ellipsis>
            {t('app.title')}
          </Typography.Text>
        </Link>
        {tenant && (
          <Tooltip title={t('header.tenant')}>
            <Tag color="green" style={{ marginInlineEnd: 0, textTransform: 'lowercase' }}>
              {tenant}
            </Tag>
          </Tooltip>
        )}
      </Flex>

      <Flex align="center" gap={12} style={{ flexShrink: 0 }}>
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
              {claims?.email ?? claims?.subject ?? t('header.account')}
            </Typography.Text>
          </Button>
        </Dropdown>
      </Flex>
    </Flex>
  )
}
