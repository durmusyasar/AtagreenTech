import type { ReactNode } from 'react'
import { Navigate, useLocation } from 'react-router-dom'
import { useAuth } from '../core/context/useAuth'

/** Oturum yoksa (veya bir 401 sonrası kapandıysa) giriş sayfasına, dönüş adresini koruyarak yönlendirir. */
export function RequireAuth({ children }: { children: ReactNode }) {
  const { isAuthenticated } = useAuth()
  const location = useLocation()

  if (!isAuthenticated) return <Navigate to="/login" replace state={{ from: location.pathname + location.search }} />

  return children
}
