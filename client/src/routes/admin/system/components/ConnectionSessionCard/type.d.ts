import type { CurrentSession } from '@/types/session'

export interface ConnectionSessionCardProps {
  session: CurrentSession | undefined
  isPending: boolean
  isFetching: boolean
  tokenExpiresLabel: string
  displayName: string
  onRefresh: () => void
}
