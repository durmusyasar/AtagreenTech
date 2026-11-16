/** Backend `PasswordRules` ile aynı: min 8, max 128, büyük/küçük harf, rakam, özel karakter. */
export const PASSWORD_MIN_LENGTH = 8
export const PASSWORD_MAX_LENGTH = 128

export interface PasswordRuleResult {
  minLength: boolean
  maxLength: boolean
  hasUpper: boolean
  hasLower: boolean
  hasDigit: boolean
  hasSpecial: boolean
  isValid: boolean
}

export function checkPasswordStrength(password: string): PasswordRuleResult {
  const minLength = password.length >= PASSWORD_MIN_LENGTH
  const maxLength = password.length <= PASSWORD_MAX_LENGTH
  const hasUpper = /[A-Z]/.test(password)
  const hasLower = /[a-z]/.test(password)
  const hasDigit = /\d/.test(password)
  const hasSpecial = /[^A-Za-z0-9]/.test(password)

  return {
    minLength,
    maxLength,
    hasUpper,
    hasLower,
    hasDigit,
    hasSpecial,
    isValid: minLength && maxLength && hasUpper && hasLower && hasDigit && hasSpecial,
  }
}
