import { useTenantTheme } from '@/core/context/useTenantTheme'
import type { BrandMarkProps } from '@/Components/BrandMark/type'

/** Tenant logosunu (ve isteğe bağlı adını) gösterir; config yoksa varsayılan favicon kullanılır. */
export function BrandMark({
  size = 24,
  showName = true,
  nameClassName = '',
  className = '',
}: BrandMarkProps) {
  const { config } = useTenantTheme()
  const src = config.logoUrl || '/favicon.svg'
  const name = config.name

  return (
    <span className={`inline-flex min-w-0 items-center gap-2.5 ${className}`.trim()}>
      <img
        src={src}
        alt={showName ? '' : name}
        width={size}
        height={size}
        className="shrink-0 object-contain"
      />
      {showName ? (
        <span
          className={`min-w-0 truncate font-semibold text-primary ${nameClassName}`.trim()}
        >
          {name}
        </span>
      ) : null}
    </span>
  )
}
