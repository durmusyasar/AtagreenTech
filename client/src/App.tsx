import { QueryClientProvider } from '@tanstack/react-query'
import { App as AntApp } from 'antd'
import { createBrowserRouter, RouterProvider } from 'react-router-dom'
import { OfflineBanner } from './components/OfflineBanner'
import { SessionHydrator } from './components/SessionHydrator'
import { AppLayout } from './components/layout/AppLayout'
import { NotFound } from './components/NotFound'
import { NotificationBridge } from './components/NotificationBridge'
import { RequireAuth } from './components/RequireAuth'
import { clearOnSessionChange, createQueryClient } from './core/api/queryClient'
import { TenantThemeProvider } from './core/context/TenantThemeProvider'
import { sessionStore } from './core/session'
import { AcceptInvitePage } from './features/auth/AcceptInvitePage'
import { ForgotPasswordPage } from './features/auth/ForgotPasswordPage'
import { LoginPage } from './features/auth/LoginPage'
import { ResetPasswordPage } from './features/auth/ResetPasswordPage'
import { VerifyEmailPage } from './features/auth/VerifyEmailPage'
import { CategoriesPage } from './features/categories/CategoriesPage'
import { DashboardPage } from './features/dashboard/DashboardPage'
import { ProductsPage } from './features/products/ProductsPage'
import { StockBalancesPage } from './features/stock/StockBalancesPage'
import { StockMovementsPage } from './features/stock/StockMovementsPage'
import { UnitsOfMeasurePage } from './features/units-of-measure/UnitsOfMeasurePage'
import { WarehousesPage } from './features/warehouses/WarehousesPage'

const queryClient = createQueryClient()
clearOnSessionChange(queryClient, sessionStore)

const router = createBrowserRouter([
  { path: '/login', element: <LoginPage /> },
  { path: '/accept-invite', element: <AcceptInvitePage /> },
  { path: '/verify-email', element: <VerifyEmailPage /> },
  { path: '/forgot-password', element: <ForgotPasswordPage /> },
  { path: '/reset-password', element: <ResetPasswordPage /> },
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
      { path: 'products', element: <ProductsPage /> },
      { path: 'warehouses', element: <WarehousesPage /> },
      { path: 'stock-movements', element: <StockMovementsPage /> },
      { path: 'stock-balances', element: <StockBalancesPage /> },
      { path: '*', element: <NotFound /> },
    ],
  },
])

export function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <TenantThemeProvider>
        <AntApp>
          <SessionHydrator>
            <NotificationBridge />
            <OfflineBanner />
            <RouterProvider router={router} />
          </SessionHydrator>
        </AntApp>
      </TenantThemeProvider>
    </QueryClientProvider>
  )
}
