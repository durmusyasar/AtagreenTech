import { RequireAuth } from '@/Components/Guards/RequireAuth'
import type { RequireAdminProps } from '@/Components/Guards/type'

/** Admin rolü gerekir; aksi halde `/unauthorized`. */
export function RequireAdmin({ children }: RequireAdminProps) {
  return <RequireAuth roles={['Admin']}>{children}</RequireAuth>
}
