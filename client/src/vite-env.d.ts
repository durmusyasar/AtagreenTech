/// <reference types="vite/client" />

interface ImportMetaEnv {
  /** API kök yolu; varsayılan `/api/v1` (aynı origin). */
  readonly VITE_API_BASE_URL?: string
  /** Adres alt alan adı içermediğinde `X-Tenant` ile gönderilen tenant (yalnızca geliştirme). */
  readonly VITE_DEV_TENANT?: string
}

interface ImportMeta {
  readonly env: ImportMetaEnv
}
