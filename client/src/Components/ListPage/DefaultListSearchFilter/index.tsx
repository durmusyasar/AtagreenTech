import { SearchOutlined } from '@ant-design/icons'
import { FormFloatingInput } from '@/Components/forms/FormFloatingInput'
import { useListPageCommands } from '@/Components/ListPage/ListPageCommandsContext'
import { useLanguage } from '@/core/context/useLanguage'
import type { DefaultListSearchFilterProps } from '@/Components/ListPage/DefaultListSearchFilter/type'

/** Built-in liste araması — `filterContent` verilmediğinde kullanılır. */
export function DefaultListSearchFilter({
  searchLabel,
  searchSubmitLabel,
}: DefaultListSearchFilterProps) {
  const { t } = useLanguage()
  const { search } = useListPageCommands()

  return (
    <FormFloatingInput
      name="search"
      className="w-full max-w-xl"
      label={searchLabel}
      autoClear
      clearLabel={t('common.clear')}
      onKeyDown={(event) => {
        if (event.key !== 'Enter') return
        event.preventDefault()
        search()
      }}
      endAdornment={
        <button
          type="button"
          tabIndex={-1}
          aria-label={searchSubmitLabel}
          onClick={() => search()}
          className="floating-input-clear"
        >
          <SearchOutlined className="text-sm" aria-hidden />
        </button>
      }
    />
  )
}
