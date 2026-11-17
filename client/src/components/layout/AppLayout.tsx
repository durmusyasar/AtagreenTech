import { Layout, theme } from 'antd'
import { useCallback, useState } from 'react'
import { Outlet } from 'react-router-dom'
import { CommandPalette } from '@/Components/command-palette/CommandPalette'
import { Footer } from '@/Components/layout/Footer'
import { Navbar } from '@/Components/layout/Navbar'
import { Sidebar } from '@/Components/Sidebar'

/**
 * Oturum açık sayfaların ortak iskeleti: sol Sidebar, üst Navbar, içerik ve Footer.
 */
export function AppLayout() {
  const [collapsed, setCollapsed] = useState(false)
  const [commandOpen, setCommandOpen] = useState(false)
  const { token } = theme.useToken()
  const openCommandPalette = useCallback(() => setCommandOpen(true), [])

  return (
    <Layout style={{ minHeight: '100vh' }}>
      <Sidebar collapsed={collapsed} onCollapse={setCollapsed} />

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
