/** Lazy route Suspense fallback — AuthPageShell / AppLayout dışında. */
export function PageLoader() {
  return (
    <div className="p-8 text-center text-sm text-slate-500 dark:text-slate-400">Yükleniyor...</div>
  )
}
