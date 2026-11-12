import type { PageRequest } from './api'

export interface Warehouse {
  id: string
  name: string
  code: string
  address: string | null
  isActive: boolean
  createdAt: string
}

export interface WarehouseQuery extends PageRequest {
  searchTerm?: string
  isActive?: boolean
}

export interface CreateWarehouseRequest {
  name: string
  code: string
  address: string | null
}

export interface UpdateWarehouseRequest extends CreateWarehouseRequest {
  isActive: boolean
}
