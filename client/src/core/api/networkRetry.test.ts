import { AxiosError, AxiosHeaders } from 'axios'
import { describe, expect, it } from 'vitest'
import { nextRetryConfig, shouldRetryNetworkError } from '@/core/api/networkRetry'

function networkError(method = 'get', networkRetryCount?: number): AxiosError {
  return new AxiosError('Network Error', 'ERR_NETWORK', {
    headers: new AxiosHeaders(),
    method,
    networkRetryCount,
  })
}

describe('shouldRetryNetworkError', () => {
  it('retries idempotent methods without a response while online', () => {
    expect(shouldRetryNetworkError(networkError('get'), true)).toBe(true)
    expect(shouldRetryNetworkError(networkError('head'), true)).toBe(true)
  })

  it('does not retry mutations or offline clients', () => {
    expect(shouldRetryNetworkError(networkError('post'), true)).toBe(false)
    expect(shouldRetryNetworkError(networkError('get'), false)).toBe(false)
  })

  it('stops after the retry limit', () => {
    expect(shouldRetryNetworkError(networkError('get', 2), true)).toBe(false)
    expect(shouldRetryNetworkError(networkError('get', 1), true)).toBe(true)
  })

  it('does not retry when the server already responded', () => {
    const error = networkError('get')
    error.response = {
      status: 500,
      data: {},
      statusText: '',
      headers: {},
      config: error.config!,
    }
    expect(shouldRetryNetworkError(error, true)).toBe(false)
  })
})

describe('nextRetryConfig', () => {
  it('increments the retry counter', () => {
    const config = nextRetryConfig({ headers: new AxiosHeaders(), networkRetryCount: 0 })
    expect(config.networkRetryCount).toBe(1)
  })
})
