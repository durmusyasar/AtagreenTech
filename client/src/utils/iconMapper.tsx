import {
  AppstoreOutlined,
  BarChartOutlined,
  ColumnWidthOutlined,
  DashboardOutlined,
  LockOutlined,
  MenuOutlined,
  SafetyCertificateOutlined,
  ShopOutlined,
  SwapOutlined,
  TagsOutlined,
  TeamOutlined,
} from '@ant-design/icons'
import type { ReactNode } from 'react'

/**
 * DB `iconKey` → Ant Design ikon. Bilinmeyen anahtarlarda nötr nokta.
 * Draft Lucide yerine mevcut shell Ant ikon seti kullanılır.
 */
const ICON_REGISTRY: Record<string, ReactNode> = {
  dashboard: <DashboardOutlined />,
  units: <ColumnWidthOutlined />,
  ruler: <ColumnWidthOutlined />,
  categories: <AppstoreOutlined />,
  tag: <TagsOutlined />,
  products: <TagsOutlined />,
  warehouses: <ShopOutlined />,
  warehouse: <ShopOutlined />,
  movements: <SwapOutlined />,
  balances: <BarChartOutlined />,
  security: <LockOutlined />,
  shield: <SafetyCertificateOutlined />,
  users: <TeamOutlined />,
  roles: <SafetyCertificateOutlined />,
  system: <AppstoreOutlined />,
  menus: <MenuOutlined />,
  menu: <MenuOutlined />,
}

function DefaultDotIcon() {
  return <span className="inline-block h-2 w-2 rounded-full bg-current opacity-60" aria-hidden />
}

export function resolveMenuIcon(iconKey: string): ReactNode {
  return ICON_REGISTRY[iconKey] ?? <DefaultDotIcon />
}
