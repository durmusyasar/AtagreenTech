import { AxiosError, AxiosHeaders, type AxiosResponse } from 'axios'
import { describe, expect, it } from 'vitest'
import { toApiError } from './apiError'

function httpError(status: number, data: unknown): AxiosError {
  const config = { headers: new AxiosHeaders() }
  const response = { status, data, statusText: '', headers: {}, config } as AxiosResponse
  return new AxiosError('Request failed', 'ERR_BAD_RESPONSE', config, null, response)
}

describe('toApiError', () => {
  it('translates a business rule (422) by resource key and parameters', () => {
    const error = toApiError(
      httpError(422, {
        errorCode: 'business_rule_violation',
        resourceKey: 'Localization.TranslationsBelowMinimum',
        parameters: ['2'],
        traceId: 'trace-1',
      }),
      'tr',
    )

    expect(error.message).toBe('En az 2 farklı dilde çeviri girilmelidir.')
    expect(error).toMatchObject({ status: 422, code: 'business_rule_violation', traceId: 'trace-1' })
    expect(toApiError(httpError(422, { resourceKey: 'Localization.DefaultLanguageMissing', parameters: ['tr'] }), 'en').message)
      .toBe('Translations must include the system default language (tr).')
  })

  it('falls back to the server detail for unknown business rules', () => {
    expect(toApiError(httpError(422, { resourceKey: 'Unknown.Rule', detail: 'Server text' }), 'tr').message).toBe('Server text')
    expect(toApiError(httpError(422, {}), 'en').message).toBe('A business rule was violated.')
  })

  it('keeps validation (400) field errors for forms', () => {
    const error = toApiError(
      httpError(400, {
        errorCode: 'validation_failed',
        errors: { Code: ['Birim kodu boş olamaz.'], 'Translations[1].Name': ['Ad boş olamaz.'] },
      }),
      'tr',
    )

    expect(error.message).toBe('Birim kodu boş olamaz. Ad boş olamaz.')
    expect(error.fieldErrors['Translations[1].Name']).toEqual(['Ad boş olamaz.'])
  })

  it.each([
    [401, 'Your session has expired, please sign in again.'],
    [403, 'You are not allowed to perform this action or access this tenant.'],
    [429, 'Too many requests, please try again shortly.'],
    [500, 'An unexpected server error occurred.'],
  ])('describes HTTP %i', (status, message) => {
    expect(toApiError(httpError(status, undefined), 'en')).toMatchObject({ status, message })
  })

  it('reports network failures without a status', () => {
    const error = toApiError(new AxiosError('Network Error', 'ERR_NETWORK'), 'tr')
    expect(error).toMatchObject({ status: null, code: 'network_error' })
  })
})
