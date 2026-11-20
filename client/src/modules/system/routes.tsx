import { Suspense } from 'react'
import type { RouteObject } from 'react-router-dom'
import { RequireAuth } from '@/Components/Guards'
import { PageLoader } from '@core/routing/PageLoader'
import { lazyNamed } from '@core/routing/lazyNamed'

const TenantsPage = lazyNamed(() => import('@modules/system/pages/TenantsPage'), 'TenantsPage')

/** SYSTEM modülü authenticated route'ları — path'ler menü kataloğu ile hizalı. */
export function getSystemRoutes(): RouteObject[] {
  return [
    {
      path: 'system/tenants',
      element: (
        <Suspense fallback={<PageLoader />}>
          <RequireAuth permissions="system.tenants.manage">
            <TenantsPage />
          </RequireAuth>
        </Suspense>
      ),
    },
  ]
}
