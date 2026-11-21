import { useLanguage } from '@/core/context/useLanguage'

/** Aktif / pasif durum rozeti — liste ve form tabloları. */
export function StatusTag({ isActive }: { isActive: boolean }) {
  const { t } = useLanguage()

  return (
    <span
      className={
        isActive
          ? 'inline-flex items-center gap-1.5 rounded-full bg-brand/12 px-2.5 py-1 text-[12px] font-semibold tracking-tight text-brand shadow-[inset_0_0_0_1px_rgb(var(--brand-primary)/0.22)]'
          : 'inline-flex items-center gap-1.5 rounded-full bg-elevated px-2.5 py-1 text-[12px] font-medium tracking-tight text-secondary shadow-[inset_0_0_0_1px_rgb(var(--border-subtle)/0.55)]'
      }
    >
      <span
        className={`h-1.5 w-1.5 rounded-full ${isActive ? 'bg-brand shadow-[0_0_0_3px_rgb(var(--brand-primary)/0.2)]' : 'bg-muted'}`}
        aria-hidden
      />
      {t(isActive ? 'list.active' : 'list.inactive')}
    </span>
  )
}
