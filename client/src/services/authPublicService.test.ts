import { beforeEach, describe, expect, it, vi } from 'vitest'

vi.mock('../core/api/axiosClient', () => ({
  axiosClient: {
    post: vi.fn(),
  },
}))

vi.mock('../core/session', () => ({
  sessionStore: {
    getSnapshot: () => ({ language: 'tr' }),
  },
}))

import { axiosClient } from '../core/api/axiosClient'
import { authPublicService } from './authPublicService'

const mockedPost = vi.mocked(axiosClient.post)

describe('authPublicService', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    mockedPost.mockResolvedValue({ data: { message: 'ok' } })
  })

  it('posts accept-invite with trimmed names', async () => {
    await authPublicService.acceptInvite({
      token: 'tok',
      password: 'Admin123!',
      firstName: ' Ada ',
      lastName: ' Lovelace ',
    })

    expect(mockedPost).toHaveBeenCalledWith(
      '/auth/accept-invite',
      { token: 'tok', password: 'Admin123!', firstName: 'Ada', lastName: 'Lovelace' },
      expect.objectContaining({ skipErrorNotification: true }),
    )
  })

  it('posts verify / resend / forgot / reset payloads', async () => {
    await authPublicService.verifyEmail('v-token')
    await authPublicService.resendVerification(' a@b.com ')
    await authPublicService.forgotPassword(' a@b.com ')
    await authPublicService.resetPassword({ token: 'r-token', newPassword: 'Admin123!' })

    expect(mockedPost).toHaveBeenNthCalledWith(
      1,
      '/auth/verify-email',
      { token: 'v-token' },
      expect.any(Object),
    )
    expect(mockedPost).toHaveBeenNthCalledWith(
      2,
      '/auth/resend-verification',
      { email: 'a@b.com' },
      expect.any(Object),
    )
    expect(mockedPost).toHaveBeenNthCalledWith(
      3,
      '/auth/forgot-password',
      { email: 'a@b.com' },
      expect.any(Object),
    )
    expect(mockedPost).toHaveBeenNthCalledWith(
      4,
      '/auth/reset-password',
      { token: 'r-token', newPassword: 'Admin123!' },
      expect.any(Object),
    )
  })
})
