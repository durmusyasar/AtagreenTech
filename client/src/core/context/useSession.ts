import { useSyncExternalStore } from 'react'
import { sessionStore, type SessionState } from '@/core/session'

export function useSession(): SessionState {
  return useSyncExternalStore(sessionStore.subscribe, sessionStore.getSnapshot)
}
