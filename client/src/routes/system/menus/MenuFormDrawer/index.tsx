import { App, Drawer, Form, Input, InputNumber, Select, Switch } from 'antd'
import { useEffect, useMemo, useState } from 'react'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import { FormErrorAlert } from '@/Components/forms/FormErrorAlert'
import { useServerErrors } from '@/Components/forms/useServerErrors'
import { TranslationTabs } from '@/Components/translations/TranslationTabs'
import { ApiError } from '@/core/api/apiError'
import { invalidate } from '@/core/api/queryClient'
import { useLanguage } from '@/core/context/useLanguage'
import { useAppMutation } from '@/core/hooks/useAppMutation'
import { queryKeys } from '@/services/queryKeys'
import {
  systemMenusQueries,
  systemMenusService,
  type SystemMenuTranslationInput,
} from '@/services/systemMenusService'
import type { MenuFormDrawerProps, MenuFormValues } from '@/routes/system/menus/type'

export function MenuFormDrawer({ open, menu, onClose, onSaved }: MenuFormDrawerProps) {
  const { language, t } = useLanguage()
  const { message } = App.useApp()
  const [form] = Form.useForm<MenuFormValues>()
  const queryClient = useQueryClient()
  const serverErrors = useServerErrors(form, {
    rootFields: ['code', 'moduleId', 'routePath', 'iconKey', 'requiredPermission'],
  })
  // API dil kodları UI Language union'ından bağımsız olabilir.
  const [activeLanguage, setActiveLanguage] = useState<string>(language)

  const languagesQuery = useQuery({ ...systemMenusQueries.languages(), enabled: open })
  const modulesQuery = useQuery({ ...systemMenusQueries.modules(), enabled: open })
  const menusQuery = useQuery({ ...systemMenusQueries.menus(), enabled: open })

  const languageCodes = useMemo(
    () => (languagesQuery.data ?? []).map((l) => l.code),
    [languagesQuery.data],
  )
  const defaultLanguage = languagesQuery.data?.find((l) => l.isDefault)?.code ?? language

  useEffect(() => {
    if (!open) return
    setActiveLanguage(defaultLanguage)
    const translations: MenuFormValues['translations'] = {}
    for (const code of languageCodes) {
      translations[code] = { name: menu?.translations?.[code] ?? '' }
    }
    form.setFieldsValue({
      code: menu?.code ?? '',
      moduleId: menu?.moduleId,
      parentId: menu?.parentId ?? null,
      routePath: menu?.routePath ?? '/',
      iconKey: menu?.iconKey ?? 'menu',
      requiredPermission: menu?.requiredPermission ?? undefined,
      displayOrder: menu?.displayOrder ?? 0,
      isActive: menu?.isActive ?? true,
      translations,
    })
  }, [open, menu, form, languageCodes, defaultLanguage])

  const save = useAppMutation({
    mutationFn: async (values: MenuFormValues) => {
      const translations: SystemMenuTranslationInput[] = languageCodes
        .map((code) => ({
          languageCode: code,
          name: values.translations?.[code]?.name?.trim() ?? '',
        }))
        .filter((row) => row.name.length > 0)

      const body = {
        code: values.code.trim(),
        moduleId: values.moduleId,
        parentId: values.parentId || null,
        routePath: values.routePath.trim(),
        iconKey: values.iconKey.trim(),
        requiredPermission: values.requiredPermission?.trim() || null,
        displayOrder: values.displayOrder,
        isActive: values.isActive,
        translations,
      }

      if (menu) await systemMenusService.updateMenu(menu.id, body)
      else await systemMenusService.createMenu(body)
    },
    onSuccess: async () => {
      message.success(menu ? t('menus.updated') : t('menus.created'))
      await invalidate(queryClient, queryKeys.systemMenus)
      await invalidate(queryClient, queryKeys.navigation)
      onSaved()
      onClose()
    },
    onError: (error) => {
      if (error instanceof ApiError) serverErrors.applyServerError(error)
    },
  })

  const parentOptions = useMemo(() => {
    const rows = menusQuery.data ?? []
    const descendantIds = new Set<string>()
    if (menu) {
      const collect = (parentId: string) => {
        for (const row of rows) {
          if (row.parentId === parentId && !descendantIds.has(row.id)) {
            descendantIds.add(row.id)
            collect(row.id)
          }
        }
      }
      collect(menu.id)
    }

    return rows
      .filter((row) => row.id !== menu?.id && !descendantIds.has(row.id))
      .map((row) => ({
        value: row.id,
        label: `${row.code} — ${row.translations[language] ?? row.translations[defaultLanguage] ?? row.code}`,
      }))
  }, [menusQuery.data, menu, language, defaultLanguage])

  const moduleOptions = useMemo(
    () =>
      (modulesQuery.data ?? []).map((mod) => ({
        value: mod.id,
        label: `${mod.code} (${mod.translations[language] ?? mod.translations[defaultLanguage] ?? mod.serviceName})`,
      })),
    [modulesQuery.data, language, defaultLanguage],
  )

  return (
    <Drawer
      open={open}
      width={480}
      title={menu ? t('menus.editTitle') : t('menus.createTitle')}
      onClose={onClose}
      destroyOnHidden
      extra={
        <ButtonSave pending={save.isPending} label={t('common.save')} onClick={() => form.submit()} />
      }
    >
      <FormErrorAlert message={serverErrors.formError} onClose={serverErrors.clearFormError} />
      <Form<MenuFormValues>
        form={form}
        layout="vertical"
        requiredMark={false}
        onFinish={(values) => {
          serverErrors.clearFormError()
          const missingDefault = !values.translations?.[defaultLanguage]?.name?.trim()
          if (missingDefault) {
            setActiveLanguage(defaultLanguage)
            message.error(t('menus.form.defaultNameRequired', defaultLanguage))
            return
          }
          save.mutate(values)
        }}
        onFinishFailed={serverErrors.clearFormError}
      >
        <Form.Item
          name="code"
          label={t('menus.form.code')}
          rules={[{ required: true, whitespace: true, message: t('menus.form.codeRequired') }]}
        >
          <Input placeholder="stock.units" disabled={Boolean(menu)} />
        </Form.Item>

        <Form.Item
          name="moduleId"
          label={t('menus.form.module')}
          rules={[{ required: true, message: t('menus.form.moduleRequired') }]}
        >
          <Select options={moduleOptions} loading={modulesQuery.isFetching} />
        </Form.Item>

        <Form.Item name="parentId" label={t('menus.form.parent')}>
          <Select allowClear options={parentOptions} loading={menusQuery.isFetching} />
        </Form.Item>

        <Form.Item
          name="routePath"
          label={t('menus.form.route')}
          rules={[{ required: true, whitespace: true, message: t('menus.form.routeRequired') }]}
        >
          <Input placeholder="/products" />
        </Form.Item>

        <Form.Item
          name="iconKey"
          label={t('menus.form.icon')}
          rules={[{ required: true, whitespace: true, message: t('menus.form.iconRequired') }]}
        >
          <Input placeholder="products" />
        </Form.Item>

        <Form.Item name="requiredPermission" label={t('menus.form.permission')}>
          <Input placeholder="stock.read" />
        </Form.Item>

        <Form.Item name="displayOrder" label={t('menus.form.displayOrder')}>
          <InputNumber className="w-full" />
        </Form.Item>

        <Form.Item name="isActive" label={t('form.isActive')} valuePropName="checked">
          <Switch />
        </Form.Item>

        {languageCodes.length > 0 ? (
          <TranslationTabs
            languages={languageCodes}
            activeLanguage={activeLanguage}
            onChange={setActiveLanguage}
            invalidLanguages={[]}
          >
            {(lang) => (
              <Form.Item
                name={['translations', lang, 'name']}
                label={t('menus.form.name')}
                rules={
                  lang === defaultLanguage
                    ? [{ required: true, whitespace: true, message: t('menus.form.nameRequired') }]
                    : undefined
                }
              >
                <Input />
              </Form.Item>
            )}
          </TranslationTabs>
        ) : null}
      </Form>
    </Drawer>
  )
}

function ButtonSave({
  pending,
  label,
  onClick,
}: {
  pending: boolean
  label: string
  onClick: () => void
}) {
  return (
    <button
      type="button"
      className="rounded-md bg-brand px-3 py-1.5 text-sm font-medium text-white disabled:opacity-60"
      disabled={pending}
      onClick={onClick}
    >
      {pending ? `${label}…` : label}
    </button>
  )
}
