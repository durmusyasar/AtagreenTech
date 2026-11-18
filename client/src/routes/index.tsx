import { lazy, Suspense, type ComponentType, type ReactNode } from 'react'
import { createBrowserRouter } from 'react-router-dom'
import { RequireAuth } from '@/Components/Guards'
import { AppLayout } from '@/Components/layout/AppLayout'

function lazyNamed<TModule extends Record<string, unknown>, TKey extends keyof TModule & string>(
  loader: () => Promise<TModule>,
  exportName: TKey,
) {
  return lazy(async () => {
    const mod = await loader()
    return { default: mod[exportName] as ComponentType }
  })
}

const LoginRoute = lazyNamed(() => import('@/routes/auth/login'), 'LoginPage')
const ForgotPasswordRoute = lazyNamed(() => import('@/routes/auth/forgot-password'), 'ForgotPasswordPage')
const ResetPasswordRoute = lazyNamed(() => import('@/routes/auth/reset-password'), 'ResetPasswordPage')
const AcceptInviteRoute = lazyNamed(() => import('@/routes/auth/accept-invite'), 'AcceptInvitePage')
const VerifyEmailRoute = lazyNamed(() => import('@/routes/auth/verify-email'), 'VerifyEmailPage')
const UnauthorizedRoute = lazyNamed(() => import('@/routes/unauthorized'), 'Unauthorized')
const NotFoundRoute = lazyNamed(() => import('@/routes/not-found'), 'NotFound')
const DashboardRoute = lazyNamed(() => import('@/routes/dashboard'), 'DashboardPage')
const UnitsOfMeasureRoute = lazyNamed(
  () => import('@/routes/catalog/units-of-measure/UnitsOfMeasurePage'),
  'UnitsOfMeasurePage',
)
const CategoriesRoute = lazyNamed(
  () => import('@/routes/catalog/categories/CategoriesPage'),
  'CategoriesPage',
)
const ProductsRoute = lazyNamed(() => import('@/routes/catalog/products/ProductsPage'), 'ProductsPage')
const WarehousesRoute = lazyNamed(
  () => import('@/routes/catalog/warehouses/WarehousesPage'),
  'WarehousesPage',
)
const StockMovementsRoute = lazyNamed(() => import('@/routes/catalog/stock-movements'), 'StockMovementsPage')
const StockBalancesRoute = lazyNamed(() => import('@/routes/catalog/stock-balances'), 'StockBalancesPage')
const SecurityRoute = lazyNamed(() => import('@/routes/account/security'), 'SecurityPage')
const AdminUsersRoute = lazyNamed(() => import('@/routes/admin/users'), 'UsersPage')
const AdminRolesRoute = lazyNamed(() => import('@/routes/admin/roles'), 'RolesPage')
const AdminSystemRoute = lazyNamed(() => import('@/routes/admin/system'), 'SystemPage')

function withSuspense(element: ReactNode) {
  return (
    <Suspense
      fallback={<div className="p-8 text-center text-sm text-slate-500 dark:text-slate-400">Yükleniyor...</div>}
    >
      {element}
    </Suspense>
  )
}

function stockRead(element: ReactNode) {
  return <RequireAuth permissions="stock.read">{element}</RequireAuth>
}

/** Domain odaklı lazy route ağacı (`createBrowserRouter`). */
export const appRouter = createBrowserRouter([
  { path: '/login', element: withSuspense(<LoginRoute />) },
  { path: '/forgot-password', element: withSuspense(<ForgotPasswordRoute />) },
  { path: '/reset-password', element: withSuspense(<ResetPasswordRoute />) },
  { path: '/accept-invite', element: withSuspense(<AcceptInviteRoute />) },
  { path: '/verify-email', element: withSuspense(<VerifyEmailRoute />) },
  { path: '/unauthorized', element: withSuspense(<UnauthorizedRoute />) },
  {
    path: '/',
    element: (
      <RequireAuth>
        <AppLayout />
      </RequireAuth>
    ),
    children: [
      { index: true, element: withSuspense(<DashboardRoute />) },
      { path: 'units-of-measure', element: withSuspense(stockRead(<UnitsOfMeasureRoute />)) },
      { path: 'categories', element: withSuspense(stockRead(<CategoriesRoute />)) },
      { path: 'products', element: withSuspense(stockRead(<ProductsRoute />)) },
      { path: 'warehouses', element: withSuspense(stockRead(<WarehousesRoute />)) },
      { path: 'stock-movements', element: withSuspense(stockRead(<StockMovementsRoute />)) },
      { path: 'stock-balances', element: withSuspense(stockRead(<StockBalancesRoute />)) },
      { path: 'account/security', element: withSuspense(<SecurityRoute />) },
      {
        path: 'admin/users',
        element: withSuspense(
          <RequireAuth roles={['Admin']} permissions="users.read">
            <AdminUsersRoute />
          </RequireAuth>,
        ),
      },
      {
        path: 'admin/roles',
        element: withSuspense(
          <RequireAuth roles={['Admin']}>
            <AdminRolesRoute />
          </RequireAuth>,
        ),
      },
      {
        path: 'admin/system',
        element: withSuspense(
          <RequireAuth permissions="system.read">
            <AdminSystemRoute />
          </RequireAuth>,
        ),
      },
      { path: '*', element: withSuspense(<NotFoundRoute />) },
    ],
  },
])
