import { axiosClient } from '../core/api/axiosClient'
import type { CurrentSession } from '../types/session'

export const sessionService = {
  /** Token'ın ve tenant'ın API tarafından kabul edildiğini doğrular. */
  async getCurrent(signal?: AbortSignal): Promise<CurrentSession> {
    const { data } = await axiosClient.get<CurrentSession>('/security-test/protected', { signal })
    return data
  },
}
