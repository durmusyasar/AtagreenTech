import { beforeEach, describe, expect, it, vi } from 'vitest'

vi.mock('../core/api/axiosClient', () => ({
  axiosClient: {
    get: vi.fn(),
    put: vi.fn(),
  },
}))

import { axiosClient } from '@/core/api/axiosClient'
import { rbacService } from '@/services/rbacService'

const mockedGet = vi.mocked(axiosClient.get)
const mockedPut = vi.mocked(axiosClient.put)

describe('rbacService', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('loads roles and permissions', async () => {
    mockedGet.mockResolvedValueOnce({ data: [] }).mockResolvedValueOnce({ data: [] })
    await rbacService.getRoles()
    await rbacService.getPermissions()
    expect(mockedGet).toHaveBeenNthCalledWith(1, '/roles', expect.any(Object))
    expect(mockedGet).toHaveBeenNthCalledWith(2, '/permissions', expect.any(Object))
  })

  it('updates role permissions and assigns user roles', async () => {
    mockedPut.mockResolvedValue({ data: { id: '1' } })
    await rbacService.updateRolePermissions('role-1', { permissions: ['users.read'] })
    await rbacService.assignUserRoles('user-1', { roles: ['Manager'] })

    expect(mockedPut).toHaveBeenNthCalledWith(
      1,
      '/roles/role-1/permissions',
      { permissions: ['users.read'] },
      expect.objectContaining({ skipErrorNotification: true }),
    )
    expect(mockedPut).toHaveBeenNthCalledWith(
      2,
      '/users/user-1/roles',
      { roles: ['Manager'] },
      expect.objectContaining({ skipErrorNotification: true }),
    )
  })
})
