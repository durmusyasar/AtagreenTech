import { Suspense, type ReactNode } from 'react'
import { createBrowserRouter } from 'react-router-dom'
import { RequireAuth } from '@/Components/Guards'
import { AppLayout } from '@/Components/layout/AppLayout'
import { modulesByScope } from '@app/moduleRegistry'
import { PageLoader } from '@core/routing/PageLoader'
import { lazyNamed } from '@core/routing/lazyNamed'

const UnauthorizedRoute = lazyNamed(() => import('@/routes/unauthorized'), 'Unauthorized')
const NotFoundRoute = lazyNamed(() => import('@/routes/not-found'), 'NotFound')

/** PR-1 geçici: henüz modüle taşınmamış domain sayfaları. */
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
const SystemMenusRoute = lazyNamed(() => import('@/routes/system/menus'), 'SystemMenusPage')

function withSuspense(element: ReactNode) {
  return <Suspense fallback={<PageLoader />}>{element}</Suspense>
}

function stockRead(element: ReactNode) {
  return <RequireAuth permissions="stock.read">{element}</RequireAuth>
}

/**
 * Modül registry + legacy catalog/admin route’larından `createBrowserRouter` ağacı.
 * Public URL path’leri değişmez.
 */
export function createAppRouter() {
  const publicRoutes = modulesByScope('public').flatMap((module) => module.getRoutes())
  const authenticatedModuleRoutes = modulesByScope('authenticated').flatMap((module) =>
    module.getRoutes(),
  )

  return createBrowserRouter([
    ...publicRoutes,
    { path: '/unauthorized', element: withSuspense(<UnauthorizedRoute />) },
    {
      path: '/',
      element: (
        <RequireAuth>
          <AppLayout />
        </RequireAuth>
      ),
      children: [
        ...authenticatedModuleRoutes,
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
        {
          path: 'system/menus',
          element: withSuspense(
            <RequireAuth permissions="system.manage">
              <SystemMenusRoute />
            </RequireAuth>,
          ),
        },
        { path: '*', element: withSuspense(<NotFoundRoute />) },
      ],
    },
  ])
}

export const appRouter = createAppRouter()
