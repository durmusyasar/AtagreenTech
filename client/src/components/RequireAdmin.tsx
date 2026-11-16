import type { ReactNode } from 'react'
import { Navigate } from 'react-router-dom'
import { useAuth } from '../core/context/useAuth'

/** Yalnızca `Admin` rolü olan kullanıcılar; değilse ana sayfaya yönlendirir. */
export function RequireAdmin({ children }: { children: ReactNode }) {
  const { isAuthenticated, claims } = useAuth()

  if (!isAuthenticated) return <Navigate to="/login" replace />

  const isAdmin = claims?.roles.some((role) => role.toLowerCase() === 'admin') ?? false
  if (!isAdmin) return <Navigate to="/" replace />

  return children
}
