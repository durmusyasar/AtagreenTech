import { beforeEach, describe, expect, it, vi } from 'vitest'

vi.mock('../core/api/axiosClient', () => ({
  axiosClient: {
    get: vi.fn(),
    post: vi.fn(),
    delete: vi.fn(),
  },
}))

vi.mock('../core/session', () => ({
  sessionStore: {
    getSnapshot: () => ({ language: 'tr' }),
  },
}))

import { axiosClient } from '../core/api/axiosClient'
import { accountSecurityService } from './accountSecurityService'

const mockedGet = vi.mocked(axiosClient.get)
const mockedPost = vi.mocked(axiosClient.post)
const mockedDelete = vi.mocked(axiosClient.delete)

describe('accountSecurityService', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('lists and revokes sessions', async () => {
    mockedGet.mockResolvedValue({ data: [] })
    mockedDelete.mockResolvedValue({})
    mockedPost.mockResolvedValue({ data: { message: 'ok' } })

    await accountSecurityService.getSessions()
    await accountSecurityService.revokeSession('s-1')
    await accountSecurityService.revokeOtherSessions()

    expect(mockedGet).toHaveBeenCalledWith('/sessions', expect.any(Object))
    expect(mockedDelete).toHaveBeenCalledWith('/sessions/s-1', expect.any(Object))
    expect(mockedPost).toHaveBeenCalledWith('/sessions/revoke-others', {}, expect.any(Object))
  })

  it('changes password and manages MFA', async () => {
    mockedPost.mockResolvedValue({
      data: { message: 'ok', sharedKey: 'ABC', qrCodeUri: 'otpauth://', recoveryCodes: ['A'] },
    })

    await accountSecurityService.changePassword({
      currentPassword: 'Old123!',
      newPassword: 'Admin123!',
    })
    await accountSecurityService.setupMfa()
    await accountSecurityService.verifyMfa('123456')
    await accountSecurityService.disableMfa('Admin123!')

    expect(mockedPost).toHaveBeenCalledWith(
      '/auth/change-password',
      { currentPassword: 'Old123!', newPassword: 'Admin123!' },
      expect.any(Object),
    )
    expect(mockedPost).toHaveBeenCalledWith('/auth/mfa/setup', {}, expect.any(Object))
    expect(mockedPost).toHaveBeenCalledWith('/auth/mfa/verify', { code: '123456' }, expect.any(Object))
    expect(mockedPost).toHaveBeenCalledWith(
      '/auth/mfa/disable',
      { password: 'Admin123!' },
      expect.any(Object),
    )
  })
})
