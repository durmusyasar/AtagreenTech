import { useAuth } from '../context/useAuth'

/** JWT `role` claim'inde Admin var mı (menü / komut paleti için). */
export function useIsAdmin(): boolean {
  const { claims } = useAuth()
  return claims?.roles.some((role) => role.toLowerCase() === 'admin') ?? false
}
