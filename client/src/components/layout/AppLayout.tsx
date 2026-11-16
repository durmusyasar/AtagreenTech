import {
  AppstoreOutlined,
  BarChartOutlined,
  ColumnWidthOutlined,
  DashboardOutlined,
  ShopOutlined,
  SwapOutlined,
  TagsOutlined,
  TeamOutlined,
} from '@ant-design/icons'
import { Layout, Menu, theme } from 'antd'
import { useCallback, useMemo, useState, type ReactNode } from 'react'
import { Outlet, useLocation, useNavigate } from 'react-router-dom'
import { BrandMark } from '../BrandMark'
import { CommandPalette } from '../command-palette/CommandPalette'
import { useLanguage } from '../../core/context/useLanguage'
import { useIsAdmin } from '../../core/hooks/useIsAdmin'
import type { MessageKey } from '../../core/i18n/messages'
import { Footer } from './Footer'
import { Navbar } from './Navbar'

const NAVIGATION: { path: string; label: MessageKey; icon: ReactNode; adminOnly?: boolean }[] = [
  { path: '/', label: 'menu.dashboard', icon: <DashboardOutlined /> },
  { path: '/units-of-measure', label: 'menu.unitsOfMeasure', icon: <ColumnWidthOutlined /> },
  { path: '/categories', label: 'menu.categories', icon: <AppstoreOutlined /> },
  { path: '/products', label: 'menu.products', icon: <TagsOutlined /> },
  { path: '/warehouses', label: 'menu.warehouses', icon: <ShopOutlined /> },
  { path: '/stock-movements', label: 'menu.stockMovements', icon: <SwapOutlined /> },
  { path: '/stock-balances', label: 'menu.stockBalances', icon: <BarChartOutlined /> },
  { path: '/admin/users', label: 'menu.users', icon: <TeamOutlined />, adminOnly: true },
]

/**
 * Oturum açık sayfaların ortak iskeleti: sol modül menüsü, üst Navbar (tenant / dil / profil),
 * içerik ve alt Footer (telif + sürüm). Rotalar `Outlet` ile buraya bağlanır.
 */
export function AppLayout() {
  const [collapsed, setCollapsed] = useState(false)
  const [commandOpen, setCommandOpen] = useState(false)
  const { pathname } = useLocation()
  const navigate = useNavigate()
  const { t } = useLanguage()
  const isAdmin = useIsAdmin()
  const { token } = theme.useToken()
  const openCommandPalette = useCallback(() => setCommandOpen(true), [])

  const visibleNav = useMemo(
    () => NAVIGATION.filter((item) => !item.adminOnly || isAdmin),
    [isAdmin],
  )
  const selected = visibleNav.filter((item) =>
    item.path === '/' ? pathname === '/' : pathname.startsWith(item.path),
  )

  return (
    <Layout style={{ minHeight: '100vh' }}>
      <Layout.Sider collapsible collapsed={collapsed} onCollapse={setCollapsed} breakpoint="lg" theme="light">
        <div className="app-logo">
          <BrandMark size={28} showName={!collapsed} nameStyle={{ color: token.colorPrimary }} />
        </div>
        <Menu
          mode="inline"
          selectedKeys={selected.map((item) => item.path)}
          onClick={({ key }) => navigate(key)}
          items={visibleNav.map((item) => ({ key: item.path, icon: item.icon, label: t(item.label) }))}
        />
      </Layout.Sider>

      <Layout style={{ minWidth: 0 }}>
        <Layout.Header
          style={{
            background: token.colorBgContainer,
            paddingInline: 24,
            borderBottom: `1px solid ${token.colorBorderSecondary}`,
            lineHeight: 'normal',
            height: 64,
            display: 'flex',
            alignItems: 'center',
          }}
        >
          <Navbar onOpenCommandPalette={openCommandPalette} />
        </Layout.Header>
        <Layout.Content style={{ margin: 24, minHeight: 280 }}>
          <Outlet />
        </Layout.Content>
        <Footer />
      </Layout>

      <CommandPalette open={commandOpen} onOpenChange={setCommandOpen} />
    </Layout>
  )
}
