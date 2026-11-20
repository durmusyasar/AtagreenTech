import type { ModuleManifest } from '@core/modules/types'
import { getSystemRoutes } from '@modules/system/routes'

export const systemModuleManifest: ModuleManifest = {
  code: 'SYSTEM',
  isCore: true,
  scope: 'authenticated',
  getRoutes: getSystemRoutes,
}
