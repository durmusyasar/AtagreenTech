import { AppstoreOutlined, ColumnWidthOutlined, DashboardOutlined } from '@ant-design/icons'
import { Layout, Menu, theme, Typography } from 'antd'
import { useState, type ReactNode } from 'react'
import { Outlet, useLocation, useNavigate } from 'react-router-dom'
import { useLanguage } from '../../core/context/useLanguage'
import type { MessageKey } from '../../core/i18n/messages'
import { AppHeader } from './AppHeader'

const NAVIGATION: { path: string; label: MessageKey; icon: ReactNode }[] = [
  { path: '/', label: 'menu.dashboard', icon: <DashboardOutlined /> },
  { path: '/units-of-measure', label: 'menu.unitsOfMeasure', icon: <ColumnWidthOutlined /> },
  { path: '/categories', label: 'menu.categories', icon: <AppstoreOutlined /> },
]

export function AppLayout() {
  const [collapsed, setCollapsed] = useState(false)
  const { pathname } = useLocation()
  const navigate = useNavigate()
  const { t } = useLanguage()
  const { token } = theme.useToken()

  const selected = NAVIGATION.filter((item) => item.path === '/' ? pathname === '/' : pathname.startsWith(item.path))

  return (
    <Layout style={{ minHeight: '100vh' }}>
      <Layout.Sider collapsible collapsed={collapsed} onCollapse={setCollapsed} breakpoint="lg" theme="light">
        <div className="app-logo">
          <img src="/favicon.svg" alt="" width={28} height={28} />
          {!collapsed && (
            <Typography.Text strong style={{ color: token.colorPrimary }}>
              {t('app.title')}
            </Typography.Text>
          )}
        </div>
        <Menu
          mode="inline"
          selectedKeys={selected.map((item) => item.path)}
          onClick={({ key }) => navigate(key)}
          items={NAVIGATION.map((item) => ({ key: item.path, icon: item.icon, label: t(item.label) }))}
        />
      </Layout.Sider>

      <Layout>
        <Layout.Header style={{ background: token.colorBgContainer, paddingInline: 24 }}>
          <AppHeader />
        </Layout.Header>
        <Layout.Content style={{ margin: 24 }}>
          <Outlet />
        </Layout.Content>
      </Layout>
    </Layout>
  )
}
