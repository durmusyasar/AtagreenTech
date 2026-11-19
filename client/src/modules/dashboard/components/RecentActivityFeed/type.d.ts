import type { RecentActivityItem } from '@modules/dashboard/types'

export interface RecentActivityFeedProps {
  items: RecentActivityItem[]
  isLoading: boolean
}
