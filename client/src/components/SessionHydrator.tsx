import { useEffect, useRef } from 'react'
import { sessionService } from '../services/sessionService'
import { sessionStore } from '../core/session'

/**
 * Sayfa yenilendiğinde sessionStorage'daki access token ile `/auth/me` doğrular.
 * 401'de axios interceptor refresh dener; o da başarısızsa oturum temizlenir.
 */
export function SessionHydrator({ children }: { children: React.ReactNode }) {
  const started = useRef(false)

  useEffect(() => {
    if (started.current) return
    started.current = true

    const { token } = sessionStore.getSnapshot()
    if (!token) return

    void sessionService.getCurrent().catch(() => {
      // Interceptor refresh'i denedikten sonra hâlâ hata varsa oturumu bırak.
      if (sessionStore.getSnapshot().token) {
        sessionStore.signOut()
      }
    })
  }, [])

  return children
}
