/** `GET /api/v1/auth/me` yanıtı (UserMeResponse). */
export interface CurrentSession {
  id: string
  email: string
  username: string
  firstName: string
  lastName: string
  tenantId: string
  roles: string[]
  permissions: string[]
  isMfaEnabled: boolean
}
