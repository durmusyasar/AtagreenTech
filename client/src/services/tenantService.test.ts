import { beforeEach, describe, expect, it, vi } from 'vitest'

vi.mock('../core/api/axiosClient', () => ({
  axiosClient: {
    get: vi.fn(),
  },
}))

import { axiosClient } from '@/core/api/axiosClient'
import { tenantService } from '@/services/tenantService'

const mocked = vi.mocked(axiosClient)

describe('tenantService.getConfig', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('GETs /tenant/config without surfacing a global error toast', async () => {
    const payload = {
      tenantId: '5803ca09-ca89-4f92-81ce-66a0477cb081',
      subdomain: 'atagreen',
      name: 'AtaGreen',
      primaryColor: '#2e7d32',
      logoUrl: '/favicon.svg',
      faviconUrl: '/favicon.svg',
    }
    mocked.get.mockResolvedValue({ data: payload })

    await expect(tenantService.getConfig()).resolves.toEqual(payload)
    expect(mocked.get).toHaveBeenCalledWith('/tenant/config', {
      signal: undefined,
      skipErrorNotification: true,
    })
  })
})
