import {
  AppstoreOutlined,
  BarChartOutlined,
  ColumnWidthOutlined,
  DashboardOutlined,
  LockOutlined,
  SafetyCertificateOutlined,
  ShopOutlined,
  SwapOutlined,
  TagsOutlined,
  TeamOutlined,
} from '@ant-design/icons'
import { Layout, Menu, theme } from 'antd'
import { useCallback, useMemo, useState } from 'react'
import { Outlet, useLocation, useNavigate } from 'react-router-dom'
import { createNavigationItems } from '../../config/navigation'
import { useLanguage } from '../../core/context/useLanguage'
import { usePermissions } from '../../core/hooks/usePermissions'
import { useThemeStore } from '../../stores/themeStore'
import { BrandMark } from '../BrandMark'
import { CommandPalette } from '../command-palette/CommandPalette'
import { Footer } from './Footer'
import { Navbar } from './Navbar'

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

/**
 * Oturum açık sayfaların ortak iskeleti: sol modül menüsü, üst Navbar (tenant / dil / profil),
 * içerik ve alt Footer (telif + sürüm). Rotalar `Outlet` ile buraya bağlanır.
 * Menü öğeleri rol/izin setine göre `createNavigationItems` üzerinden filtrelenir.
 */
export function AppLayout() {
  const [collapsed, setCollapsed] = useState(false)
  const [commandOpen, setCommandOpen] = useState(false)
  const { pathname } = useLocation()
  const navigate = useNavigate()
  const { t } = useLanguage()
  const access = usePermissions()
  const { token } = theme.useToken()
  const resolvedTheme = useThemeStore((state) => state.resolvedTheme)
  const openCommandPalette = useCallback(() => setCommandOpen(true), [])

  const visibleNav = useMemo(
    () =>
      createNavigationItems(NAV_ICONS).filter((item) =>
        access.canAccess({ roles: item.roles, permissions: item.permissions }),
      ),
    [access],
  )
  const selected = visibleNav.filter((item) =>
    item.path === '/' ? pathname === '/' : pathname.startsWith(item.path),
  )

  return (
    <Layout style={{ minHeight: '100vh' }}>
      <Layout.Sider
        collapsible
        collapsed={collapsed}
        onCollapse={setCollapsed}
        breakpoint="lg"
        theme={resolvedTheme === 'dark' ? 'dark' : 'light'}
      >
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
