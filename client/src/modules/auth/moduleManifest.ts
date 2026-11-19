import type { ModuleManifest } from '@core/modules/types'
import { getAuthRoutes } from '@modules/auth/routes'

export const authModuleManifest: ModuleManifest = {
  code: 'AUTH',
  isCore: true,
  scope: 'public',
  getRoutes: getAuthRoutes,
}
