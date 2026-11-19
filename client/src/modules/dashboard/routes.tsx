import { Suspense } from 'react'
import type { RouteObject } from 'react-router-dom'
import { PageLoader } from '@core/routing/PageLoader'
import { lazyNamed } from '@core/routing/lazyNamed'

const DashboardPage = lazyNamed(
  () => import('@modules/dashboard/pages/DashboardPage'),
  'DashboardPage',
)

/** Authenticated index (`/`) — path değişmez. */
export function getDashboardRoutes(): RouteObject[] {
  return [
    {
      index: true,
      element: (
        <Suspense fallback={<PageLoader />}>
          <DashboardPage />
        </Suspense>
      ),
    },
  ]
}
