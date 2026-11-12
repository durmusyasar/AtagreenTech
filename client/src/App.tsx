import { App as AntApp, ConfigProvider } from 'antd'
import enUS from 'antd/locale/en_US'
import trTR from 'antd/locale/tr_TR'
import { createBrowserRouter, RouterProvider } from 'react-router-dom'
import { AppLayout } from './components/layout/AppLayout'
import { NotFound } from './components/NotFound'
import { NotificationBridge } from './components/NotificationBridge'
import { RequireAuth } from './components/RequireAuth'
import { useLanguage } from './core/context/useLanguage'
import type { Language } from './core/i18n/languages'
import { LoginPage } from './features/auth/LoginPage'
import { CategoriesPage } from './features/categories/CategoriesPage'
import { DashboardPage } from './features/dashboard/DashboardPage'
import { StockBalancesPage } from './features/stock/StockBalancesPage'
import { StockMovementsPage } from './features/stock/StockMovementsPage'
import { UnitsOfMeasurePage } from './features/units-of-measure/UnitsOfMeasurePage'
import { WarehousesPage } from './features/warehouses/WarehousesPage'

const ANTD_LOCALES = { tr: trTR, en: enUS } satisfies Record<Language, unknown>

// Ant Design'ın koyu yeşilden türettiği açık tonlar griye kaçıp seçili öğelerde metin kontrastını düşürüyor.
const THEME = { token: { colorPrimary: '#2e7d32', colorPrimaryBg: '#e8f5e9', colorPrimaryBgHover: '#c8e6c9' } }

const router = createBrowserRouter([
  { path: '/login', element: <LoginPage /> },
  {
    path: '/',
    element: (
      <RequireAuth>
        <AppLayout />
      </RequireAuth>
    ),
    children: [
      { index: true, element: <DashboardPage /> },
      { path: 'units-of-measure', element: <UnitsOfMeasurePage /> },
      { path: 'categories', element: <CategoriesPage /> },
      { path: 'warehouses', element: <WarehousesPage /> },
      { path: 'stock-movements', element: <StockMovementsPage /> },
      { path: 'stock-balances', element: <StockBalancesPage /> },
      { path: '*', element: <NotFound /> },
    ],
  },
])

export function App() {
  const { language } = useLanguage()

  return (
    <ConfigProvider locale={ANTD_LOCALES[language]} theme={THEME}>
      <AntApp>
        <NotificationBridge />
        <RouterProvider router={router} />
      </AntApp>
    </ConfigProvider>
  )
}
