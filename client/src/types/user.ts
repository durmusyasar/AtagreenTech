import type { PageRequest } from '@/types/api'

/** `GET /api/v1/users` satırı (`UserDto`). */
export interface UserItem {
  id: string
  email: string
  username: string
  firstName: string
  lastName: string
  isActive: boolean
  isEmailVerified: boolean
  createdAt: string
  roles: string[]
}

export interface UserListQuery extends PageRequest {
  searchTerm?: string
  isActive?: boolean
}

export interface InviteUserRequest {
  email: string
  firstName: string
  lastName: string
  role: string
}

export interface UpdateUserRequest {
  firstName: string
  lastName: string
  roles: string[]
}

/** Seed edilen tenant rolleri (Invite / Update). */
export const TENANT_ROLES = ['Admin', 'Manager', 'Viewer'] as const
export type TenantRole = (typeof TENANT_ROLES)[number]
