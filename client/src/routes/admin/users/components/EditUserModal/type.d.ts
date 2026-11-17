import type { UpdateUserRequest, UserItem } from '@/types/user'

export interface EditUserModalProps {
  user: UserItem | null
  open: boolean
  onClose: () => void
  onSuccess: () => void
}

export interface EditUserForm extends UpdateUserRequest {}
