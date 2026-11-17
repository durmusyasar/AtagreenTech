import { useCallback, useMemo } from 'react'
import { authService } from '@/services/authService'
import { readTokenClaims, type TokenClaims } from '@/core/jwt'
import { sessionStore, type SignInSession } from '@/core/session'
import { useSession } from '@/core/context/useSession'

export interface AuthContextValue {
  token: string | null
  refreshToken: string | null
  claims: TokenClaims | null
  isAuthenticated: boolean
  signIn: (session: SignInSession) => void
  signOut: () => Promise<void>
}

export function useAuth(): AuthContextValue {
  const { token, refreshToken } = useSession()

  const signOut = useCallback(async () => {
    await authService.logout()
  }, [])

  return useMemo(() => {
    const claims = token ? readTokenClaims(token) : null
    return {
      token,
      refreshToken,
      claims,
      isAuthenticated: token !== null,
      signIn: sessionStore.signIn,
      signOut,
    }
  }, [token, refreshToken, signOut])
}
