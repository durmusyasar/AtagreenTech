import { queryOptions } from '@tanstack/react-query'
import { axiosClient } from '@/core/api/axiosClient'
import type { PagedResult } from '@/types/api'
import type { InviteUserRequest, UpdateUserRequest, UserItem, UserListQuery } from '@/types/user'
import { queryKeys } from '@/services/queryKeys'

const BASE = '/users'

export const userService = {
  async list(query: UserListQuery, signal?: AbortSignal): Promise<PagedResult<UserItem>> {
    const { data } = await axiosClient.get<PagedResult<UserItem>>(BASE, { params: query, signal })
    return data
  },

  async invite(payload: InviteUserRequest): Promise<{ message: string }> {
    const { data } = await axiosClient.post<{ message: string }>(
      `${BASE}/invite`,
      {
        email: payload.email.trim(),
        firstName: payload.firstName.trim(),
        lastName: payload.lastName.trim(),
        role: payload.role,
      },
      { skipErrorNotification: true },
    )
    return data
  },

  async update(id: string, payload: UpdateUserRequest): Promise<UserItem> {
    const { data } = await axiosClient.put<UserItem>(
      `${BASE}/${id}`,
      {
        firstName: payload.firstName.trim(),
        lastName: payload.lastName.trim(),
        roles: payload.roles,
      },
      { skipErrorNotification: true },
    )
    return data
  },

  async activate(id: string): Promise<void> {
    await axiosClient.patch(`${BASE}/${id}/activate`)
  },

  async deactivate(id: string): Promise<void> {
    await axiosClient.patch(`${BASE}/${id}/deactivate`)
  },

  async remove(id: string): Promise<void> {
    await axiosClient.delete(`${BASE}/${id}`)
  },
}

export const userQueries = {
  list: (query: UserListQuery) =>
    queryOptions({
      queryKey: [...queryKeys.users, 'list', query],
      queryFn: ({ signal }) => userService.list(query, signal),
    }),
}
