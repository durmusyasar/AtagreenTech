import { GlobalOutlined, LogoutOutlined, SearchOutlined, UserOutlined } from '@ant-design/icons'
import { Button, Dropdown, Flex, Select, Tag, Tooltip, Typography } from 'antd'
import { Link } from 'react-router-dom'
import { BrandMark } from '../BrandMark'
import { commandPaletteShortcutLabel } from '../command-palette/commandPaletteSearch'
import { useAuth } from '../../core/context/useAuth'
import { useLanguage } from '../../core/context/useLanguage'
import { useTenant } from '../../core/context/useTenant'
import { LANGUAGE_LABELS, SUPPORTED_LANGUAGES, type Language } from '../../core/i18n/languages'

interface NavbarProps {
  onOpenCommandPalette?: () => void
}

/**
 * Üst çubuk: tenant markası, komut paleti, dil seçici (TR/EN) ve kullanıcı / çıkış.
 * Modül linkleri sol menüdedir (`AppLayout` sider); burada yalnızca oturum ve bağlam kontrolleri vardır.
 */
export function Navbar({ onOpenCommandPalette }: NavbarProps) {
  const { claims, signOut } = useAuth()
  const { tenant } = useTenant()
  const { language, setLanguage, t } = useLanguage()
  const shortcut = commandPaletteShortcutLabel()

  return (
    <Flex align="center" justify="space-between" gap={16} style={{ height: '100%', width: '100%' }}>
      <Flex align="center" gap={12} style={{ minWidth: 0 }}>
        <Link to="/" style={{ display: 'flex', alignItems: 'center', color: 'inherit', minWidth: 0 }}>
          <BrandMark size={24} />
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
        {onOpenCommandPalette && (
          <Tooltip title={`${t('header.commandPalette')} (${shortcut})`}>
            <Button
              type="text"
              icon={<SearchOutlined />}
              onClick={onOpenCommandPalette}
              aria-label={t('header.commandPalette')}
            >
              <Typography.Text type="secondary" style={{ fontSize: 12 }}>
                {shortcut}
              </Typography.Text>
            </Button>
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
            items: [
              {
                key: 'sign-out',
                icon: <LogoutOutlined />,
                label: t('header.signOut'),
                onClick: () => {
                  void signOut()
                },
              },
            ],
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
