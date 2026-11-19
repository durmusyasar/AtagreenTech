import type { ModuleManifest } from '@core/modules/types'
import { getDashboardRoutes } from '@modules/dashboard/routes'

export const dashboardModuleManifest: ModuleManifest = {
  code: 'GENERAL',
  isCore: true,
  scope: 'authenticated',
  getRoutes: getDashboardRoutes,
}
