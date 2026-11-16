import { beforeEach, describe, expect, it, vi } from 'vitest'

vi.mock('../core/api/axiosClient', () => ({
  axiosClient: {
    get: vi.fn(),
    post: vi.fn(),
    put: vi.fn(),
    patch: vi.fn(),
    delete: vi.fn(),
  },
}))

import { axiosClient } from '../core/api/axiosClient'
import { userService } from './userService'

const mockedGet = vi.mocked(axiosClient.get)
const mockedPost = vi.mocked(axiosClient.post)
const mockedPut = vi.mocked(axiosClient.put)

describe('userService', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('lists users with query params', async () => {
    mockedGet.mockResolvedValue({
      data: { items: [], pageNumber: 1, pageSize: 20, totalCount: 0, totalPages: 0, hasPreviousPage: false, hasNextPage: false },
    })

    await userService.list({ pageNumber: 2, pageSize: 15, searchTerm: 'ada', isActive: true })

    expect(mockedGet).toHaveBeenCalledWith(
      '/users',
      expect.objectContaining({
        params: { pageNumber: 2, pageSize: 15, searchTerm: 'ada', isActive: true },
      }),
    )
  })

  it('invites and updates with trimmed fields', async () => {
    mockedPost.mockResolvedValue({ data: { message: 'ok' } })
    mockedPut.mockResolvedValue({
      data: {
        id: '1',
        email: 'a@b.com',
        username: 'a',
        firstName: 'Ada',
        lastName: 'Lovelace',
        isActive: true,
        isEmailVerified: false,
        createdAt: '2026-01-01',
        roles: ['Viewer'],
      },
    })

    await userService.invite({
      email: ' a@b.com ',
      firstName: ' Ada ',
      lastName: ' Lovelace ',
      role: 'Viewer',
    })
    await userService.update('1', { firstName: ' Ada ', lastName: ' Lovelace ', roles: ['Manager'] })

    expect(mockedPost).toHaveBeenCalledWith(
      '/users/invite',
      { email: 'a@b.com', firstName: 'Ada', lastName: 'Lovelace', role: 'Viewer' },
      expect.objectContaining({ skipErrorNotification: true }),
    )
    expect(mockedPut).toHaveBeenCalledWith(
      '/users/1',
      { firstName: 'Ada', lastName: 'Lovelace', roles: ['Manager'] },
      expect.objectContaining({ skipErrorNotification: true }),
    )
  })
})
