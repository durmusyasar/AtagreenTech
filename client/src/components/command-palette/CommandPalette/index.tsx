import {
  AppstoreOutlined,
  BarChartOutlined,
  ColumnWidthOutlined,
  DashboardOutlined,
  GlobalOutlined,
  LockOutlined,
  PlusOutlined,
  SafetyCertificateOutlined,
  ShopOutlined,
  SwapOutlined,
  TagsOutlined,
  TeamOutlined,
} from '@ant-design/icons'
import { Empty, Flex, Input, Modal, Typography, theme, type InputRef } from 'antd'
import {
  useCallback,
  useEffect,
  useMemo,
  useRef,
  useState,
  type KeyboardEvent as ReactKeyboardEvent,
} from 'react'
import { useNavigate } from 'react-router-dom'
import { createNavigationItems } from '@/config/navigation'
import { useLanguage } from '@/core/context/useLanguage'
import { usePermissions } from '@/core/hooks/usePermissions'
import { filterByQuery, isCommandPaletteToggle } from '@/Components/command-palette/commandPaletteSearch'
import type { CommandPaletteProps, PaletteCommand } from '@/Components/command-palette/CommandPalette/type'

const NAV_ICONS = {
  dashboard: <DashboardOutlined />,
  units: <ColumnWidthOutlined />,
  categories: <AppstoreOutlined />,
  products: <TagsOutlined />,
  warehouses: <ShopOutlined />,
  movements: <SwapOutlined />,
  balances: <BarChartOutlined />,
  security: <LockOutlined />,
  users: <TeamOutlined />,
  roles: <SafetyCertificateOutlined />,
}

const NAV_KEYWORDS: Record<string, string> = {
  '/': 'home overview genel bakış dashboard',
  '/units-of-measure': 'uom unit ölçü birim',
  '/categories': 'category kategori',
  '/products': 'product ürün sku',
  '/warehouses': 'warehouse depo',
  '/stock-movements': 'movement hareket transfer stock',
  '/stock-balances': 'balance bakiye inventory',
  '/account/security': 'security güvenlik mfa password şifre session oturum',
  '/admin/users': 'users kullanıcı admin invite davet',
  '/admin/roles': 'roles permissions rbac rol izin',
}

/**
 * Ctrl/Cmd+K ile açılan komuta paleti: sayfa gezinmesi, hızlı oluşturma ve dil değiştirme.
 * Menü rotaları rol/izin filtresiyle navigation config'ten türetilir.
 */
export function CommandPalette({ open, onOpenChange }: CommandPaletteProps) {
  const navigate = useNavigate()
  const { language, setLanguage, t } = useLanguage()
  const access = usePermissions()
  const { token } = theme.useToken()
  const [query, setQuery] = useState('')
  const [activeIndex, setActiveIndex] = useState(0)
  const inputRef = useRef<InputRef>(null)

  const setOpen = useCallback(
    (next: boolean) => {
      if (!next) {
        setQuery('')
        setActiveIndex(0)
      }
      onOpenChange(next)
    },
    [onOpenChange],
  )

  const go = useCallback(
    (path: string) => {
      navigate(path)
      setOpen(false)
    },
    [navigate, setOpen],
  )

  const commands = useMemo<PaletteCommand[]>(() => {
    const pageCommands = createNavigationItems(NAV_ICONS)
      .filter((item) => access.canAccess({ roles: item.roles, permissions: item.permissions }))
      .map(
        (item): PaletteCommand => ({
          id: item.path,
          titleKey: item.label,
          categoryKey: 'command.category.pages',
          keywords: NAV_KEYWORDS[item.path] ?? '',
          icon: item.icon,
          run: () => go(item.path),
        }),
      )

    const actionCommands: PaletteCommand[] = []
    if (access.can('stock.write')) {
      actionCommands.push({
        id: 'new-product',
        titleKey: 'command.newProduct',
        categoryKey: 'command.category.actions',
        keywords: 'new create ekle ürün product',
        icon: <PlusOutlined />,
        run: () => go('/products?action=new'),
      })
    }

    return [
      ...pageCommands,
      ...actionCommands,
      {
        id: 'lang-toggle',
        titleKey: 'command.toggleLanguage',
        categoryKey: 'command.category.system',
        keywords: 'language dil tr en english türkçe',
        icon: <GlobalOutlined />,
        run: () => {
          setLanguage(language === 'tr' ? 'en' : 'tr')
          setOpen(false)
        },
      },
    ]
  }, [access, go, language, setLanguage, setOpen])

  const searchable = useMemo(
    () =>
      commands.map((command) => ({
        ...command,
        title: t(command.titleKey),
        categoryLabel: t(command.categoryKey),
        searchText: `${t(command.titleKey)} ${t(command.categoryKey)} ${command.keywords}`,
      })),
    [commands, t],
  )

  const filtered = useMemo(() => filterByQuery(searchable, query), [searchable, query])
  const safeIndex = filtered.length === 0 ? 0 : Math.min(activeIndex, filtered.length - 1)

  useEffect(() => {
    const onKeyDown = (event: KeyboardEvent) => {
      if (!isCommandPaletteToggle(event)) return
      event.preventDefault()
      setOpen(!open)
    }
    window.addEventListener('keydown', onKeyDown)
    return () => window.removeEventListener('keydown', onKeyDown)
  }, [open, setOpen])

  useEffect(() => {
    if (!open) return
    const timer = window.setTimeout(() => inputRef.current?.focus(), 0)
    return () => window.clearTimeout(timer)
  }, [open])

  const onInputKeyDown = (event: ReactKeyboardEvent<HTMLInputElement>) => {
    if (event.key === 'ArrowDown') {
      event.preventDefault()
      setActiveIndex(filtered.length === 0 ? 0 : (safeIndex + 1) % filtered.length)
      return
    }
    if (event.key === 'ArrowUp') {
      event.preventDefault()
      setActiveIndex(filtered.length === 0 ? 0 : (safeIndex - 1 + filtered.length) % filtered.length)
      return
    }
    if (event.key === 'Enter') {
      event.preventDefault()
      filtered[safeIndex]?.run()
    }
  }

  return (
    <Modal
      open={open}
      onCancel={() => setOpen(false)}
      footer={null}
      closable={false}
      width={560}
      destroyOnHidden
      className="command-palette-modal"
      styles={{ body: { padding: 0 } }}
      style={{ top: 72 }}
    >
      <div style={{ padding: '12px 16px', borderBottom: `1px solid ${token.colorBorderSecondary}` }}>
        <Input
          ref={inputRef}
          size="large"
          variant="borderless"
          placeholder={t('command.placeholder')}
          value={query}
          onChange={(event) => {
            setQuery(event.target.value)
            setActiveIndex(0)
          }}
          onKeyDown={onInputKeyDown}
          aria-label={t('command.placeholder')}
          aria-activedescendant={filtered[safeIndex] ? `command-item-${filtered[safeIndex].id}` : undefined}
        />
      </div>

      <div style={{ maxHeight: 360, overflow: 'auto', padding: 8 }} role="listbox" aria-label={t('command.title')}>
        {filtered.length === 0 ? (
          <Empty image={Empty.PRESENTED_IMAGE_SIMPLE} description={t('command.empty')} style={{ padding: 24 }} />
        ) : (
          filtered.map((item, index) => {
            const active = index === safeIndex
            return (
              <button
                key={item.id}
                id={`command-item-${item.id}`}
                type="button"
                role="option"
                aria-selected={active}
                className="command-palette-item"
                onMouseEnter={() => setActiveIndex(index)}
                onClick={item.run}
                style={{
                  display: 'flex',
                  width: '100%',
                  alignItems: 'center',
                  gap: 12,
                  padding: '10px 12px',
                  border: 'none',
                  borderRadius: token.borderRadius,
                  background: active ? token.colorPrimaryBg : 'transparent',
                  color: 'inherit',
                  cursor: 'pointer',
                  textAlign: 'left',
                }}
              >
                <span style={{ fontSize: 18, color: token.colorPrimary, lineHeight: 1 }}>{item.icon}</span>
                <Flex vertical style={{ flex: 1, minWidth: 0 }}>
                  <Typography.Text strong ellipsis>
                    {item.title}
                  </Typography.Text>
                  <Typography.Text type="secondary" style={{ fontSize: 12 }}>
                    {item.categoryLabel}
                  </Typography.Text>
                </Flex>
                <Typography.Text type="secondary" style={{ fontSize: 11, flexShrink: 0 }}>
                  {t('command.go')}
                </Typography.Text>
              </button>
            )
          })
        )}
      </div>
    </Modal>
  )
}
