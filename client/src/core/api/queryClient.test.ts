import { QueryClient } from '@tanstack/react-query'
import { describe, expect, it, vi } from 'vitest'
import { ApiError } from './apiError'
import { clearOnSessionChange, invalidate, shouldRetry } from './queryClient'

const apiError = (status: number | null) => new ApiError({ message: 'x', status, code: 'x' })

describe('shouldRetry', () => {
  it('retries network and server errors once', () => {
    expect(shouldRetry(0, apiError(null))).toBe(true)
    expect(shouldRetry(0, apiError(503))).toBe(true)
    expect(shouldRetry(1, apiError(503))).toBe(false)
  })

  it('does not retry client errors', () => {
    for (const status of [400, 401, 403, 404, 422]) expect(shouldRetry(0, apiError(status))).toBe(false)
  })
})

describe('clearOnSessionChange', () => {
  function fakeSession(initial: { token: string | null; tenant: string | null }) {
    let state = initial
    const listeners = new Set<() => void>()
    return {
      getSnapshot: () => state,
      subscribe: (listener: () => void) => {
        listeners.add(listener)
        return () => listeners.delete(listener)
      },
      set(next: Partial<typeof state>) {
        state = { ...state, ...next }
        listeners.forEach((listener) => listener())
      },
    }
  }

  it('clears the cache when the token or tenant changes, not on other session changes', () => {
    const queryClient = new QueryClient()
    const clear = vi.spyOn(queryClient, 'clear')
    const session = fakeSession({ token: 'a', tenant: 'acme' })
    clearOnSessionChange(queryClient, session)

    session.set({})
    expect(clear).not.toHaveBeenCalled()

    session.set({ tenant: 'globex' })
    session.set({ token: null })
    expect(clear).toHaveBeenCalledTimes(2)
  })
})

describe('invalidate', () => {
  it('marks every query under the given keys stale', async () => {
    const queryClient = new QueryClient()
    queryClient.setQueryData(['stock', 'balances', {}], 1)
    queryClient.setQueryData(['warehouses', 'all', {}], 2)

    await invalidate(queryClient, ['stock'])

    expect(queryClient.getQueryState(['stock', 'balances', {}])?.isInvalidated).toBe(true)
    expect(queryClient.getQueryState(['warehouses', 'all', {}])?.isInvalidated).toBe(false)
  })
})
