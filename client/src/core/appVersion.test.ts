import { describe, expect, it } from 'vitest'
import { APP_VERSION } from './appVersion'

describe('APP_VERSION', () => {
  it('exposes the product version shown in the footer', () => {
    expect(APP_VERSION).toBe('3.0.0')
  })
})
