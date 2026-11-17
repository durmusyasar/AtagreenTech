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
import { useMemo } from 'react'
import { useLocation, useNavigate } from 'react-router-dom'
import { BrandMark } from '@/Components/BrandMark'
import type { SidebarProps } from '@/Components/Sidebar/type'
import { createNavigationItems } from '@/config/navigation'
import { useLanguage } from '@/core/context/useLanguage'
import { usePermissions } from '@/core/hooks/usePermissions'
import { useThemeStore } from '@/stores/themeStore'

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

/** Sol navigasyon menüsü (AppLayout sider). */
export function Sidebar({ collapsed, onCollapse }: SidebarProps) {
  const { pathname } = useLocation()
  const navigate = useNavigate()
  const { t } = useLanguage()
  const access = usePermissions()
  const { token } = theme.useToken()
  const resolvedTheme = useThemeStore((state) => state.resolvedTheme)

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
    <Layout.Sider
      collapsible
      collapsed={collapsed}
      onCollapse={onCollapse}
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
  )
}
