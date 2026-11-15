import { QueryClient } from '@tanstack/react-query'
import { beforeEach, describe, expect, it, vi } from 'vitest'

vi.mock('../api/axiosClient', () => ({
  axiosClient: {
    get: vi.fn(),
    post: vi.fn(),
    put: vi.fn(),
    patch: vi.fn(),
    delete: vi.fn(),
  },
}))

import { axiosClient } from '../api/axiosClient'
import { createGetQueryFn, executeMutationRequest } from './appRequest'
import { fetchAppQuery } from './useAppLazyQuery'

const mocked = vi.mocked(axiosClient)

describe('createGetQueryFn', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('GETs the url with params and abort signal', async () => {
    mocked.get.mockResolvedValue({ data: { items: [1] } })
    const signal = new AbortController().signal

    await expect(createGetQueryFn<{ items: number[] }>('/products', { pageSize: 20 })({ signal })).resolves.toEqual({
      items: [1],
    })

    expect(mocked.get).toHaveBeenCalledWith('/products', expect.objectContaining({ params: { pageSize: 20 }, signal }))
  })
})

describe('executeMutationRequest', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('POSTs variables as the body by default', async () => {
    mocked.post.mockResolvedValue({ data: 'new-id' })

    await expect(
      executeMutationRequest({ url: '/products', variables: { sku: 'A' }, skipErrorNotification: true }),
    ).resolves.toBe('new-id')

    expect(mocked.post).toHaveBeenCalledWith('/products', { sku: 'A' }, { skipErrorNotification: true })
  })

  it('resolves a dynamic url and maps the body for PUT', async () => {
    mocked.put.mockResolvedValue({ data: undefined })

    await executeMutationRequest({
      url: (vars: { id: string }) => `/products/${vars.id}`,
      method: 'put',
      variables: { id: 'p1', sku: 'A', isActive: true },
      body: ({ sku, isActive }) => ({ sku, isActive }),
    })

    expect(mocked.put).toHaveBeenCalledWith('/products/p1', { sku: 'A', isActive: true }, { skipErrorNotification: undefined })
  })

  it('DELETEs without a body unless body is provided', async () => {
    mocked.delete.mockResolvedValue({ data: undefined })

    await executeMutationRequest({
      url: (product: { id: string }) => `/products/${product.id}`,
      method: 'delete',
      variables: { id: 'p1' },
    })
    expect(mocked.delete).toHaveBeenCalledWith('/products/p1', { skipErrorNotification: undefined })

    await executeMutationRequest({
      url: '/products/p1',
      method: 'delete',
      variables: { reason: 'x' },
      body: (vars) => vars,
    })
    expect(mocked.delete).toHaveBeenCalledWith('/products/p1', { skipErrorNotification: undefined, data: { reason: 'x' } })
  })
})

describe('fetchAppQuery', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('merges default params, builds the key and caches the result', async () => {
    mocked.get.mockResolvedValue({ data: { name: 'Pipe' } })
    const queryClient = new QueryClient()

    const result = await fetchAppQuery(
      queryClient,
      {
        url: ({ id }) => `/products/${id}`,
        queryKey: ({ id, language }) => ['products', 'detail', id, language],
        params: ({ language }) => ({ language }),
      },
      { id: 'p1', language: 'en' },
    )

    expect(result).toEqual({ name: 'Pipe' })
    expect(mocked.get).toHaveBeenCalledWith(
      '/products/p1',
      expect.objectContaining({ params: { language: 'en' } }),
    )
    expect(queryClient.getQueryData(['products', 'detail', 'p1', 'en'])).toEqual({ name: 'Pipe' })
  })
})
