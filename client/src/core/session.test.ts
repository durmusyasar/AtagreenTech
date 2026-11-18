/** @vitest-environment happy-dom */
import { beforeEach, describe, expect, it } from 'vitest'
import { sessionStore } from '@/core/session'

const ACCESS_TOKEN_KEY = 'atagreen.accessToken'
const REFRESH_TOKEN_KEY = 'atagreen.refreshToken'
const REMEMBER_ME_KEY = 'atagreen.rememberMe'

describe('sessionStore rememberMe', () => {
  beforeEach(() => {
    sessionStore.signOut()
    localStorage.clear()
    sessionStorage.clear()
  })

  it('stores tokens in sessionStorage when rememberMe is false', () => {
    sessionStore.signIn({
      accessToken: 'access-session',
      refreshToken: 'refresh-session',
      expiresAt: '2026-11-17T12:00:00Z',
      rememberMe: false,
    })

    expect(sessionStorage.getItem(ACCESS_TOKEN_KEY)).toBe('access-session')
    expect(sessionStorage.getItem(REFRESH_TOKEN_KEY)).toBe('refresh-session')
    expect(localStorage.getItem(ACCESS_TOKEN_KEY)).toBeNull()
    expect(localStorage.getItem(REMEMBER_ME_KEY)).toBeNull()
    expect(sessionStore.getSnapshot().rememberMe).toBe(false)
  })

  it('stores tokens in localStorage when rememberMe is true', () => {
    sessionStore.signIn({
      accessToken: 'access-local',
      refreshToken: 'refresh-local',
      expiresAt: '2026-11-17T12:00:00Z',
      rememberMe: true,
    })

    expect(localStorage.getItem(ACCESS_TOKEN_KEY)).toBe('access-local')
    expect(localStorage.getItem(REFRESH_TOKEN_KEY)).toBe('refresh-local')
    expect(localStorage.getItem(REMEMBER_ME_KEY)).toBe('1')
    expect(sessionStorage.getItem(ACCESS_TOKEN_KEY)).toBeNull()
    expect(sessionStore.getSnapshot().rememberMe).toBe(true)
  })

  it('keeps refresh tokens in the same storage as rememberMe', () => {
    sessionStore.signIn({
      accessToken: 'a1',
      refreshToken: 'r1',
      rememberMe: true,
    })
    sessionStore.setTokens('a2', 'r2', '2026-11-17T13:00:00Z')

    expect(localStorage.getItem(ACCESS_TOKEN_KEY)).toBe('a2')
    expect(localStorage.getItem(REFRESH_TOKEN_KEY)).toBe('r2')
    expect(sessionStorage.getItem(ACCESS_TOKEN_KEY)).toBeNull()
  })

  it('clears both storages on signOut', () => {
    sessionStore.signIn({
      accessToken: 'a',
      refreshToken: 'r',
      rememberMe: true,
    })
    sessionStore.signOut()

    expect(localStorage.getItem(ACCESS_TOKEN_KEY)).toBeNull()
    expect(sessionStorage.getItem(ACCESS_TOKEN_KEY)).toBeNull()
    expect(localStorage.getItem(REMEMBER_ME_KEY)).toBeNull()
    expect(sessionStore.getSnapshot().token).toBeNull()
    expect(sessionStore.getSnapshot().rememberMe).toBe(false)
  })
})
