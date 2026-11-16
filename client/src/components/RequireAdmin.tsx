import type { ReactNode } from 'react'
import { RequireAuth } from './RequireAuth'

/** Admin rolü gerekir; aksi halde `/unauthorized`. */
export function RequireAdmin({ children }: { children: ReactNode }) {
  return <RequireAuth roles={['Admin']}>{children}</RequireAuth>
}
