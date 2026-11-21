import { EyeInvisibleOutlined, EyeOutlined, HolderOutlined, LockOutlined } from '@ant-design/icons'
import { Drawer } from 'antd'
import { useMemo, useRef, useState, type DragEvent } from 'react'
import {
  COLUMN_WIDTH_BOUNDS,
  clampColumnWidth,
  isColumnLocked,
  resolveColumnKey,
  resolveColumnLabel,
} from '@/Components/ListPage/columnPreferences'
import type { ListColumnPreference } from '@/Components/ListPage/type'
import type { ListColumnSettingsDrawerProps } from '@/Components/ListPage/ListColumnSettingsDrawer/type'
import { useLanguage } from '@/core/context/useLanguage'

/**
 * ConnectedListPage kolon ayarları — sürükle-bırak, göz ile görünürlük, genişlik.
 * Yoğun tek yüzey; tercihler parent’ta localStorage’a yazılır.
 */
export function ListColumnSettingsDrawer<TRecord extends object>({
  open,
  columns,
  preferences,
  onClose,
  onSave,
  onReset,
}: ListColumnSettingsDrawerProps<TRecord>) {
  const { t } = useLanguage()
  const editorKey = `${open ? '1' : '0'}:${JSON.stringify(preferences)}`

  return (
    <Drawer
      rootClassName="list-column-drawer"
      title={
        <h2 className="pr-8 text-[1.15rem] font-semibold tracking-tight text-primary">
          {t('listPage.columnSettingsTitle')}
        </h2>
      }
      open={open}
      onClose={onClose}
      width={380}
      destroyOnHidden
      classNames={{ body: 'custom-scrollbar' }}
      footer={null}
    >
      {open ? (
        <ColumnSettingsEditor
          key={editorKey}
          columns={columns}
          preferences={preferences}
          onClose={onClose}
          onSave={onSave}
          onReset={onReset}
        />
      ) : null}
    </Drawer>
  )
}

function ColumnSettingsEditor<TRecord extends object>({
  columns,
  preferences,
  onClose,
  onSave,
  onReset,
}: Omit<ListColumnSettingsDrawerProps<TRecord>, 'open'>) {
  const { t } = useLanguage()
  const [draft, setDraft] = useState<ListColumnPreference[]>(() =>
    preferences.map((item) => ({ ...item })),
  )
  const dragIndexRef = useRef<number | null>(null)
  const [draggingIndex, setDraggingIndex] = useState<number | null>(null)
  const [overIndex, setOverIndex] = useState<number | null>(null)

  const columnByKey = useMemo(() => {
    const map = new Map<string, (typeof columns)[number]>()
    for (const column of columns) {
      const key = resolveColumnKey(column)
      if (key) map.set(key, column)
    }
    return map
  }, [columns])

  const visibleCount = draft.filter((item) => item.visible).length
  const canSave = visibleCount > 0

  const reorder = (from: number, to: number) => {
    if (from === to || to < 0 || to >= draft.length) return
    setDraft((prev) => {
      const next = [...prev]
      const [removed] = next.splice(from, 1)
      if (!removed) return prev
      next.splice(to, 0, removed)
      return next
    })
  }

  const setVisible = (key: string, visible: boolean) => {
    setDraft((prev) =>
      prev.map((item) => {
        if (item.key !== key) return item
        const column = columnByKey.get(key)
        if (column && isColumnLocked(column) && !visible) return item
        if (!visible && visibleCount <= 1 && item.visible) return item
        return { ...item, visible }
      }),
    )
  }

  const setWidth = (key: string, raw: string) => {
    const parsed = raw.trim() === '' ? undefined : Number(raw)
    setDraft((prev) =>
      prev.map((item) =>
        item.key === key
          ? { ...item, width: parsed === undefined ? undefined : clampColumnWidth(parsed) }
          : item,
      ),
    )
  }

  const onDragStart = (index: number) => (event: DragEvent<HTMLButtonElement>) => {
    dragIndexRef.current = index
    setDraggingIndex(index)
    event.dataTransfer.effectAllowed = 'move'
    event.dataTransfer.setData('text/plain', String(index))
  }

  const onDragOver = (index: number) => (event: DragEvent<HTMLLIElement>) => {
    event.preventDefault()
    event.dataTransfer.dropEffect = 'move'
    if (overIndex !== index) setOverIndex(index)
  }

  const onDrop = (index: number) => (event: DragEvent<HTMLLIElement>) => {
    event.preventDefault()
    const from = dragIndexRef.current
    dragIndexRef.current = null
    setDraggingIndex(null)
    setOverIndex(null)
    if (from === null) return
    reorder(from, index)
  }

  const onDragEnd = () => {
    dragIndexRef.current = null
    setDraggingIndex(null)
    setOverIndex(null)
  }

  return (
    <div className="list-column-shell">
      <div className="list-column-hero">
        <p className="text-[13px] leading-relaxed text-secondary">
          {t('listPage.columnSettingsHint')}
        </p>
        <div className="flex flex-wrap items-center gap-2">
          <span className="list-column-stats">
            <span className="list-column-stats-dot" aria-hidden />
            {t('listPage.columnVisibleCount', String(visibleCount), String(draft.length))}
          </span>
          {!canSave ? (
            <span className="text-xs text-amber-500">{t('listPage.columnSettingsNeedOne')}</span>
          ) : null}
        </div>
      </div>

      <ul className="list-column-list">
        {draft.map((item, index) => {
          const column = columnByKey.get(item.key)
          if (!column) return null
          const locked = isColumnLocked(column)
          const label = resolveColumnLabel(column, item.key)
          const toggleDisabled = locked || (item.visible && visibleCount <= 1)
          const rowClass = [
            'list-column-row',
            item.visible ? '' : 'list-column-row--hidden',
            draggingIndex === index ? 'list-column-row--dragging' : '',
            overIndex === index && draggingIndex !== index ? 'list-column-row--over' : '',
          ]
            .filter(Boolean)
            .join(' ')

          return (
            <li
              key={item.key}
              className={rowClass}
              onDragOver={onDragOver(index)}
              onDrop={onDrop(index)}
              onDragLeave={() => {
                if (overIndex === index) setOverIndex(null)
              }}
            >
              <span className="list-column-index" aria-hidden>
                {index + 1}
              </span>

              <button
                type="button"
                className="list-column-drag"
                draggable
                aria-label={t('listPage.columnDrag', label)}
                onDragStart={onDragStart(index)}
                onDragEnd={onDragEnd}
              >
                <HolderOutlined aria-hidden />
              </button>

              <span className="list-column-label">{label}</span>

              <div className="list-column-meta">
                {item.visible ? (
                  <label className="list-column-width" htmlFor={`col-width-${item.key}`}>
                    <input
                      id={`col-width-${item.key}`}
                      type="number"
                      min={COLUMN_WIDTH_BOUNDS.min}
                      max={COLUMN_WIDTH_BOUNDS.max}
                      value={item.width ?? ''}
                      placeholder="—"
                      aria-label={t('listPage.columnWidthLabel', label)}
                      onChange={(event) => setWidth(item.key, event.target.value)}
                    />
                    <span>{t('listPage.columnWidth')}</span>
                  </label>
                ) : null}

                {locked ? (
                  <span
                    className="list-column-lock"
                    title={t('listPage.columnLocked')}
                    aria-label={t('listPage.columnLocked')}
                  >
                    <LockOutlined aria-hidden />
                  </span>
                ) : (
                  <button
                    type="button"
                    role="switch"
                    className="list-column-eye"
                    aria-checked={item.visible}
                    aria-label={t('listPage.columnVisible', label)}
                    disabled={toggleDisabled}
                    onClick={() => setVisible(item.key, !item.visible)}
                  >
                    {item.visible ? (
                      <EyeOutlined aria-hidden />
                    ) : (
                      <EyeInvisibleOutlined aria-hidden />
                    )}
                  </button>
                )}
              </div>
            </li>
          )
        })}
      </ul>

      <div className="list-column-footer">
        <button type="button" className="list-page-btn list-page-btn--quiet" onClick={onReset}>
          {t('listPage.columnSettingsReset')}
        </button>
        <div className="flex items-center gap-2">
          <button type="button" className="list-page-btn list-page-btn--ghost" onClick={onClose}>
            {t('common.cancel')}
          </button>
          <button
            type="button"
            className="list-page-btn list-page-btn--primary"
            disabled={!canSave}
            onClick={() => onSave(draft)}
          >
            {t('common.save')}
          </button>
        </div>
      </div>
    </div>
  )
}
