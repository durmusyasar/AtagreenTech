import type { RouteObject } from 'react-router-dom'

/** Tenant / menü aboneliği ile hizalı modül kodları (#66). */
export type ModuleCode = 'AUTH' | 'GENERAL' | 'STOCK' | 'IAM' | 'SYSTEM'

/**
 * Modül sözleşmesi — yalnızca tip; kayıt defteri `src/app/moduleRegistry` üzerindedir
 * (core → modules import yasağı).
 *
 * `getRoutes` senkron tutulur; sayfa bileşenleri React.lazy ile code-split edilir.
 * İleride #66 için tüm modül chunk’ı dinamik import edilebilir.
 */
export interface ModuleManifest {
  code: ModuleCode
  /** true ise tenant aboneliği sorgulanmadan her zaman register edilir. */
  isCore?: boolean
  /** Authenticated layout altında mı yoksa public root’ta mı. */
  scope: 'public' | 'authenticated'
  getRoutes: () => RouteObject[]
}
