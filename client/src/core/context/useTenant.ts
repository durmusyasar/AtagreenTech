import { useSession } from './useSession'

export interface TenantContextValue {
  tenant: string | null
  /** `true` ise tenant adresin alt alan adından gelir ve arayüzden değiştirilemez. */
  isTenantFromHost: boolean
}

export function useTenant(): TenantContextValue {
  const { tenant, isTenantFromHost } = useSession()
  return { tenant, isTenantFromHost }
}
