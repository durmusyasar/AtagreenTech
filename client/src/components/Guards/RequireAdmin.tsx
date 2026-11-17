import { RequireAuth } from './RequireAuth'
import type { RequireAdminProps } from './type'

/** Admin rolü gerekir; aksi halde `/unauthorized`. */
export function RequireAdmin({ children }: RequireAdminProps) {
  return <RequireAuth roles={['Admin']}>{children}</RequireAuth>
}
