/** `GET /api/v1/security-test/protected` yanıtı: token'ın ve tenant'ın API tarafından nasıl görüldüğü. */
export interface CurrentSession {
  message: string
  userId: string | null
  email: string | null
  roles: string[]
  tenant: string | null
  timestamp: string
}
