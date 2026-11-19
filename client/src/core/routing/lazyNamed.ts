import { lazy, type ComponentType } from 'react'

/** Named export’u `default` gibi React.lazy ile yükler. */
export function lazyNamed<TModule extends Record<string, unknown>, TKey extends keyof TModule & string>(
  loader: () => Promise<TModule>,
  exportName: TKey,
) {
  return lazy(async () => {
    const mod = await loader()
    return { default: mod[exportName] as ComponentType }
  })
}
