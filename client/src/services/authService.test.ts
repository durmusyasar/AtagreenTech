import { beforeEach, describe, expect, it, vi } from 'vitest'

vi.mock('../core/api/axiosClient', () => ({
  axiosClient: {
    get: vi.fn(),
  },
}))

vi.mock('../core/session', () => ({
  sessionStore: {
    signIn: vi.fn(),
    signOut: vi.fn(),
    getSnapshot: () => ({ token: null, tenant: 'acme', isTenantFromHost: true, language: 'tr' }),
  },
}))

import { ApiError } from '../core/api/apiError'
import { axiosClient } from '../core/api/axiosClient'
import { sessionStore } from '../core/session'
import { authService } from './authService'

const mockedGet = vi.mocked(axiosClient.get)
const mockedSignIn = vi.mocked(sessionStore.signIn)

function token(payload: object): string {
  const encode = (value: object) =>
    btoa(String.fromCharCode(...new TextEncoder().encode(JSON.stringify(value))))
      .replaceAll('+', '-')
      .replaceAll('/', '_')
      .replaceAll('=', '')
  return `${encode({ alg: 'HS256', typ: 'JWT' })}.${encode(payload)}.signature`
}

describe('authService.login', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('rejects malformed and expired tokens before calling the API', async () => {
    await expect(authService.login({ accessToken: 'not-a-jwt' })).rejects.toMatchObject({ code: 'invalid_token' })
    await expect(
      authService.login({ accessToken: token({ email: 'a@b.c', exp: 1 }) }),
    ).rejects.toMatchObject({ code: 'token_expired' })
    expect(mockedGet).not.toHaveBeenCalled()
    expect(mockedSignIn).not.toHaveBeenCalled()
  })

  it('validates with the protected endpoint then stores the session', async () => {
    const accessToken = token({
      sub: 'test-user',
      email: 'test@acme.local',
      role: ['Admin'],
      tenant_id: '11111111-1111-1111-1111-111111111111',
      exp: Math.floor(Date.now() / 1000) + 3600,
    })
    mockedGet.mockResolvedValue({
      data: {
        message: 'ok',
        userId: 'test-user',
        email: 'test@acme.local',
        roles: ['Admin'],
        tenant: 'acme',
        timestamp: new Date().toISOString(),
      },
    })

    await expect(authService.login({ accessToken })).resolves.toEqual({
      token: accessToken,
      userName: 'test@acme.local',
      tenant: 'acme',
      roles: ['Admin'],
    })

    expect(mockedGet).toHaveBeenCalledWith(
      '/security-test/protected',
      expect.objectContaining({
        headers: { Authorization: `Bearer ${accessToken}` },
        skipErrorNotification: true,
      }),
    )
    expect(mockedSignIn).toHaveBeenCalledWith(accessToken)
  })

  it('does not store the session when the API rejects the token', async () => {
    const accessToken = token({ email: 'test@acme.local', exp: Math.floor(Date.now() / 1000) + 3600 })
    mockedGet.mockRejectedValue(new ApiError({ message: 'unauthorized', status: 401, code: 'http_401' }))

    await expect(authService.login({ accessToken })).rejects.toMatchObject({ status: 401 })
    expect(mockedSignIn).not.toHaveBeenCalled()
  })
})
