import type { RecentActivityItem } from '@/routes/dashboard/type'

export interface RecentActivityFeedProps {
  items: RecentActivityItem[]
  isLoading: boolean
}
