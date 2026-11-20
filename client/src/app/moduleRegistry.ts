import type { ModuleManifest } from '@core/modules/types'
import { authModuleManifest } from '@modules/auth/moduleManifest'
import { dashboardModuleManifest } from '@modules/dashboard/moduleManifest'
import { systemModuleManifest } from '@modules/system/moduleManifest'

/**
 * Merkezi modül kaydı — tek yerde modules → app köprüsü.
 * core bu dosyayı import etmez (#143 dependency rule).
 */
export const REGISTERED_MODULES: ModuleManifest[] = [
  authModuleManifest,
  dashboardModuleManifest,
  systemModuleManifest,
]

export function modulesByScope(scope: ModuleManifest['scope']): ModuleManifest[] {
  return REGISTERED_MODULES.filter((module) => module.scope === scope)
}
