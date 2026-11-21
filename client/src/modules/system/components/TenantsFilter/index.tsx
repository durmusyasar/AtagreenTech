import { DefaultListSearchFilter } from '@/Components/ListPage/DefaultListSearchFilter'
import { useLanguage } from '@/core/context/useLanguage'

/**
 * Tenant listesi filtre alanları — ConnectedListPage FormProvider çocuğu.
 * Enter / arama ikonu `useListPageCommands().search` tetikler.
 */
export function TenantsFilter() {
  const { t } = useLanguage()

  return (
    <DefaultListSearchFilter
      searchLabel={t('tenants.search')}
      searchSubmitLabel={t('tenants.searchSubmit')}
    />
  )
}
