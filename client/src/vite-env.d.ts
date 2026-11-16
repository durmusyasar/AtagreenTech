/// <reference types="vite/client" />

interface ImportMetaEnv {
  /** API kök yolu; varsayılan `/api/v1` (aynı origin). */
  readonly VITE_API_BASE_URL?: string
  /**
   * Adres alt alan adı içermediğinde `X-Tenant` ile gönderilen varsayılan tenant. Yalnızca geliştirme sunucusunda
   * (`npm run dev`) kullanılır; derlenmiş pakette yok sayılır.
   */
  readonly VITE_DEV_TENANT?: string
}

interface ImportMeta {
  readonly env: ImportMetaEnv
}
