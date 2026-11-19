import { beforeEach, describe, expect, it, vi } from 'vitest'

vi.mock('../core/api/axiosClient', () => ({
  axiosClient: {
    post: vi.fn(),
    get: vi.fn(),
  },
}))

vi.mock('../core/session', () => ({
  sessionStore: {
    signIn: vi.fn(),
    signOut: vi.fn(),
    setTokens: vi.fn(),
    getSnapshot: () => ({
      token: null,
      refreshToken: 'refresh-token',
      expiresAt: null,
      rememberMe: false,
      tenant: 'atagreen',
      isTenantFromHost: true,
      language: 'tr',
    }),
  },
}))

vi.mock('./sessionService', () => ({
  sessionService: {
    getCurrent: vi.fn(),
  },
}))

import { ApiError } from '@/core/api/apiError'
import { axiosClient } from '@/core/api/axiosClient'
import { sessionStore } from '@/core/session'
import { AuthErrorCodes } from '@/types/auth'
import { authService } from '@/services/authService'
import { sessionService } from '@/services/sessionService'

const mockedPost = vi.mocked(axiosClient.post)
const mockedSignIn = vi.mocked(sessionStore.signIn)
const mockedSignOut = vi.mocked(sessionStore.signOut)
const mockedGetMe = vi.mocked(sessionService.getCurrent)

describe('authService.login', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('stores tokens and loads /auth/me on success', async () => {
    mockedPost.mockResolvedValue({
      data: {
        accessToken: 'access',
        refreshToken: 'refresh',
        expiresAt: '2026-11-16T12:00:00Z',
      },
    })
    mockedGetMe.mockResolvedValue({
      id: '11111111-1111-1111-1111-111111111111',
      email: 'admin@omnisync.com',
      username: 'admin',
      firstName: 'System',
      lastName: 'Admin',
      tenantId: '5803ca09-ca89-4f92-81ce-66a0477cb081',
      roles: ['Admin'],
      permissions: ['users.read'],
      isMfaEnabled: false,
    })

    await expect(
      authService.login({ email: 'admin@omnisync.com', password: 'Admin123!', rememberMe: true }),
    ).resolves.toMatchObject({
      userName: 'admin@omnisync.com',
      roles: ['Admin'],
      accessToken: 'access',
      refreshToken: 'refresh',
    })

    expect(mockedPost).toHaveBeenCalledWith(
      '/auth/login',
      expect.objectContaining({
        email: 'admin@omnisync.com',
        password: 'Admin123!',
        rememberMe: true,
      }),
      expect.objectContaining({ skipErrorNotification: true }),
    )
    expect(mockedSignIn).toHaveBeenCalledWith({
      accessToken: 'access',
      refreshToken: 'refresh',
      expiresAt: '2026-11-16T12:00:00Z',
      rememberMe: true,
    })
    expect(mockedGetMe).toHaveBeenCalled()
  })

  it('propagates AUTH_MFA_REQUIRED without storing a session', async () => {
    mockedPost.mockRejectedValue(
      new ApiError({
        message: 'İki aşamalı doğrulama kodu gereklidir.',
        status: 401,
        code: AuthErrorCodes.MfaRequired,
      }),
    )

    await expect(
      authService.login({ email: 'admin@omnisync.com', password: 'Admin123!' }),
    ).rejects.toMatchObject({ code: AuthErrorCodes.MfaRequired })

    expect(mockedSignIn).not.toHaveBeenCalled()
    expect(mockedGetMe).not.toHaveBeenCalled()
  })

  it('signs out when /auth/me fails after tokens are stored', async () => {
    mockedPost.mockResolvedValue({
      data: {
        accessToken: 'access',
        refreshToken: 'refresh',
        expiresAt: '2026-11-16T12:00:00Z',
      },
    })
    mockedGetMe.mockRejectedValue(
      new ApiError({ message: 'me failed', status: 500, code: 'http_500' }),
    )

    await expect(
      authService.login({ email: 'admin@omnisync.com', password: 'Admin123!' }),
    ).rejects.toMatchObject({ code: 'http_500' })

    expect(mockedSignIn).toHaveBeenCalled()
    expect(mockedSignOut).toHaveBeenCalled()
  })
})

describe('authService.logout', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('posts refresh token then signs out', async () => {
    mockedPost.mockResolvedValue({ data: {} })

    await authService.logout()

    expect(mockedPost).toHaveBeenCalledWith(
      '/auth/logout',
      { refreshToken: 'refresh-token' },
      expect.objectContaining({ skipErrorNotification: true }),
    )
    expect(mockedSignOut).toHaveBeenCalled()
  })

  it('signs out even when logout request fails', async () => {
    mockedPost.mockRejectedValue(new Error('network'))

    await authService.logout()

    expect(mockedSignOut).toHaveBeenCalled()
  })
})
