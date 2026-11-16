import axios from 'axios'
import { sessionStore } from '../session'
import type { AuthTokens } from '../../types/auth'

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || '/api/v1'

/**
 * Tek kullanımlık refresh rotation. Axios interceptor döngüsünden kaçınmak için ham `axios` kullanır.
 */
export async function refreshAccessToken(refreshToken: string): Promise<AuthTokens> {
  const { data } = await axios.post<AuthTokens>(`${API_BASE_URL}/auth/refresh`, { refreshToken })
  sessionStore.setTokens(data.accessToken, data.refreshToken, data.expiresAt)
  return data
}
