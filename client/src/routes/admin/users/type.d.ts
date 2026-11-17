import type { UserItem } from '@/types/user'

export type StatusFilter = 'all' | 'active' | 'inactive'
export type UserStatusFilter = StatusFilter

export interface UsersTableProps {
  users: UserItem[] | undefined
  loading: boolean
  pageNumber: number
  pageSize: number
  totalCount: number
  currentUserId?: string
  onPageChange: (page: number) => void
  onEdit: (user: UserItem) => void
  onToggleActive: (user: UserItem) => void
  onDelete: (user: UserItem) => void
  toggleActivePendingId?: string
  deletePendingId?: string
}

export interface UserDetailDrawerProps {
  user: UserItem | null
  open: boolean
  onClose: () => void
  onEdit: (user: UserItem) => void
}
