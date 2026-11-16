import { QueryClientProvider } from '@tanstack/react-query'
import { App as AntApp } from 'antd'
import type { ReactNode } from 'react'
import { createBrowserRouter, RouterProvider } from 'react-router-dom'
import { OfflineBanner } from './components/OfflineBanner'
import { SessionHydrator } from './components/SessionHydrator'
import { AppLayout } from './components/layout/AppLayout'
import { NotFound } from './components/NotFound'
import { NotificationBridge } from './components/NotificationBridge'
import { RequireAuth } from './components/RequireAuth'
import { Unauthorized } from './components/Unauthorized'
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
import { RolesPage } from './features/roles/RolesPage'
import { SecurityPage } from './features/security/SecurityPage'
import { UsersPage } from './features/users/UsersPage'
import { WarehousesPage } from './features/warehouses/WarehousesPage'

const queryClient = createQueryClient()
clearOnSessionChange(queryClient, sessionStore)

function stockRead(element: ReactNode) {
  return <RequireAuth permissions="stock.read">{element}</RequireAuth>
}

const router = createBrowserRouter([
  { path: '/login', element: <LoginPage /> },
  { path: '/accept-invite', element: <AcceptInvitePage /> },
  { path: '/verify-email', element: <VerifyEmailPage /> },
  { path: '/forgot-password', element: <ForgotPasswordPage /> },
  { path: '/reset-password', element: <ResetPasswordPage /> },
  { path: '/unauthorized', element: <Unauthorized /> },
  {
    path: '/',
    element: (
      <RequireAuth>
        <AppLayout />
      </RequireAuth>
    ),
    children: [
      { index: true, element: <DashboardPage /> },
      { path: 'units-of-measure', element: stockRead(<UnitsOfMeasurePage />) },
      { path: 'categories', element: stockRead(<CategoriesPage />) },
      { path: 'products', element: stockRead(<ProductsPage />) },
      { path: 'warehouses', element: stockRead(<WarehousesPage />) },
      { path: 'stock-movements', element: stockRead(<StockMovementsPage />) },
      { path: 'stock-balances', element: stockRead(<StockBalancesPage />) },
      { path: 'account/security', element: <SecurityPage /> },
      {
        path: 'admin/users',
        element: (
          <RequireAuth roles={['Admin']} permissions="users.read">
            <UsersPage />
          </RequireAuth>
        ),
      },
      {
        path: 'admin/roles',
        element: (
          <RequireAuth roles={['Admin']}>
            <RolesPage />
          </RequireAuth>
        ),
      },
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
