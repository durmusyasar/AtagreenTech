import { usePermissions } from './usePermissions'

/** JWT `role` claim'inde Admin var mı (menü / komut paleti için). */
export function useIsAdmin(): boolean {
  const { hasRole } = usePermissions()
  return hasRole('Admin')
}
