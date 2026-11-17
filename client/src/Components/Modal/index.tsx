import type { ModalProps } from '@/Components/Modal/type'

/** Hafif Tailwind modal kabuğu; Ant Design Modal alternatifi ortak ekranlar için. */
export function Modal({
  isOpen,
  title,
  onClose,
  children,
  maxWidthClass = 'max-w-md',
}: ModalProps) {
  if (!isOpen) return null

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/70 px-4">
      <div
        className={`w-full ${maxWidthClass} space-y-4 rounded-2xl border border-slate-200 bg-white p-6 shadow-xl dark:border-slate-800 dark:bg-slate-900`}
      >
        <div className="flex items-center justify-between border-b border-slate-200 pb-3 dark:border-slate-800">
          <h2 className="text-lg font-semibold text-slate-900 dark:text-slate-100">{title}</h2>
          <button
            type="button"
            onClick={onClose}
            className="text-sm text-slate-400 hover:text-slate-700 dark:hover:text-slate-200"
            aria-label="Close"
          >
            ✕
          </button>
        </div>
        {children}
      </div>
    </div>
  )
}
