export interface UnitOfMeasureTranslation {
  languageCode: string
  name: string
  symbol: string
}

/** `name` ve `symbol` istenen dile göre (yoksa ana dil, varsayılan dil, ilk çeviri) çözümlenmiştir. */
export interface UnitOfMeasure {
  id: string
  code: string
  name: string
  symbol: string
  isActive: boolean
  translations: UnitOfMeasureTranslation[]
}

export interface CreateUnitOfMeasureRequest {
  code: string
  translations: UnitOfMeasureTranslation[]
}

export interface UpdateUnitOfMeasureRequest extends CreateUnitOfMeasureRequest {
  isActive: boolean
}
