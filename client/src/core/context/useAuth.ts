import { useMemo } from 'react'
import { readTokenClaims, type TokenClaims } from '../jwt'
import { sessionStore } from '../session'
import { useSession } from './useSession'

export interface AuthContextValue {
  token: string | null
  claims: TokenClaims | null
  isAuthenticated: boolean
  signIn: (token: string, tenant?: string) => void
  signOut: () => void
}

export function useAuth(): AuthContextValue {
  const { token } = useSession()

  return useMemo(() => {
    const claims = token ? readTokenClaims(token) : null
    return {
      token,
      claims,
      isAuthenticated: token !== null,
      signIn: sessionStore.signIn,
      signOut: sessionStore.signOut,
    }
  }, [token])
}
