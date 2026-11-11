import type { PageRequest } from './api'

export interface CategoryTranslation {
  languageCode: string
  name: string
  description?: string | null
}

/** `name`, `description` ve `parentCategoryName` istenen dile göre çözümlenmiştir. */
export interface Category {
  id: string
  code: string
  name: string
  description: string | null
  parentCategoryId: string | null
  parentCategoryName: string | null
  isActive: boolean
  createdAt: string
  translations: CategoryTranslation[]
}

export interface CategoryQuery extends PageRequest {
  searchTerm?: string
  parentCategoryId?: string
  isActive?: boolean
}

export interface CreateCategoryRequest {
  code: string
  translations: CategoryTranslation[]
  parentCategoryId?: string | null
}

export interface UpdateCategoryRequest extends CreateCategoryRequest {
  isActive: boolean
}
