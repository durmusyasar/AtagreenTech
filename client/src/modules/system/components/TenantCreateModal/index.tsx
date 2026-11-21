import { zodResolver } from '@hookform/resolvers/zod'
import { Modal } from 'antd'
import { useEffect } from 'react'
import { FormProvider, useForm } from 'react-hook-form'
import { FormFloatingInput } from '@/Components/forms/FormFloatingInput'
import { useLanguage } from '@/core/context/useLanguage'
import {
  tenantCreateSchema,
  type TenantCreateSchemaValues,
} from '@modules/system/components/TenantCreateModal/schema'
import type { TenantCreateModalProps } from '@modules/system/components/TenantCreateModal/type'

const defaultValues: TenantCreateSchemaValues = {
  name: '',
  slug: '',
  adminEmail: '',
}

/** Platform tenant oluşturma modalı — FloatingInput + Zod. */
export function TenantCreateModal({ open, confirming, onClose, onSubmit }: TenantCreateModalProps) {
  const { t } = useLanguage()
  const form = useForm<TenantCreateSchemaValues>({
    resolver: zodResolver(tenantCreateSchema),
    defaultValues,
    mode: 'onBlur',
    reValidateMode: 'onChange',
  })

  useEffect(() => {
    if (!open) form.reset(defaultValues)
  }, [open, form])

  const handleValid = (values: TenantCreateSchemaValues) => {
    onSubmit({
      name: values.name.trim(),
      slug: values.slug.trim().toLowerCase(),
      adminEmail: values.adminEmail?.trim() || undefined,
    })
  }

  return (
    <Modal
      title={t('tenants.createTitle')}
      open={open}
      onCancel={onClose}
      destroyOnHidden
      footer={
        <div className="flex justify-end gap-2">
          <button
            type="button"
            onClick={onClose}
            className="inline-flex h-9 items-center justify-center rounded-xl border border-subtle bg-surface px-4 text-sm font-medium text-secondary transition-colors hover:border-brand/30 hover:text-primary dark:border-white/10"
          >
            {t('common.cancel')}
          </button>
          <button
            type="button"
            disabled={confirming}
            onClick={() => void form.handleSubmit(handleValid)()}
            className="inline-flex h-9 items-center justify-center rounded-xl bg-brand px-4 text-sm font-semibold text-white shadow-[0_10px_24px_-12px_rgb(var(--brand-primary)/0.85)] transition-[background-color,box-shadow] hover:bg-brand-hover disabled:opacity-50"
          >
            {confirming ? t('tenants.creating') : t('tenants.create')}
          </button>
        </div>
      }
    >
      <FormProvider {...form}>
        <form
          className="space-y-4 pt-1"
          onSubmit={(event) => {
            event.preventDefault()
            void form.handleSubmit(handleValid)()
          }}
          noValidate
        >
          <FormFloatingInput<TenantCreateSchemaValues>
            name="name"
            label={t('tenants.form.name')}
            autoFocus
            autoClear
            maxLength={128}
          />
          <div className="space-y-1.5">
            <FormFloatingInput<TenantCreateSchemaValues>
              name="slug"
              label={t('tenants.form.slug')}
              autoClear
              maxLength={63}
              transformValue={(value) => value.toLowerCase().replace(/[^a-z0-9-]/g, '')}
            />
            <p className="px-0.5 text-xs leading-relaxed text-secondary">{t('tenants.form.slugHint')}</p>
          </div>
          <div className="space-y-1.5">
            <FormFloatingInput<TenantCreateSchemaValues>
              name="adminEmail"
              type="email"
              label={t('tenants.form.adminEmail')}
              autoClear
              maxLength={256}
            />
            <p className="px-0.5 text-xs leading-relaxed text-secondary">
              {t('tenants.form.adminEmailHint')}
            </p>
          </div>
        </form>
      </FormProvider>
    </Modal>
  )
}
