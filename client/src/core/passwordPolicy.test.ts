import { describe, expect, it } from 'vitest'
import { checkPasswordStrength } from '@/core/passwordPolicy'

describe('checkPasswordStrength', () => {
  it('accepts a strong password', () => {
    expect(checkPasswordStrength('Admin123!').isValid).toBe(true)
  })

  it('rejects passwords missing a rule', () => {
    expect(checkPasswordStrength('short1!').isValid).toBe(false)
    expect(checkPasswordStrength('alllowercase1!').hasUpper).toBe(false)
    expect(checkPasswordStrength('ALLUPPERCASE1!').hasLower).toBe(false)
    expect(checkPasswordStrength('NoDigits!!').hasDigit).toBe(false)
    expect(checkPasswordStrength('NoSpecial1').hasSpecial).toBe(false)
  })
})
