# ConnectedListPage (liste kabuğu)

Paylaşılan liste UX’i: `client/src/Components/ListPage/`.

## Sorumluluklar

| Özellik | Davranış |
|---|---|
| Filtre formu | Kabuk `FormProvider` + Zod (`filterSchema` / `defaultFilter`) |
| `filterContent` | Alanlar (ör. `TenantsFilter`); yoksa built-in `DefaultListSearchFilter` |
| Ara / Enter | `useListPageCommands().search` → `onNeedData` |
| Filtre temizle | Form reset + sıralama sıfır; filtre persistence silinir |
| `…` menü | Filtre kriterlerini kaydet · Kolon ayarlarını güncelle |
| Filtre kaydet | `localStorage` `atagreen.listPage.<storageKey>` |
| Kolon ayarları | Görünürlük / sıra / genişlik → `atagreen.listPage.columns.<storageKey>` |
| Sayfalama | Ant Design Table; `pageSize` değiştirilebilir |
| Sıralama | Sütun `sorter` + `sortOrder`; meta `sorter.field/order` |
| Detail | `onDetailPage` create/edit/view |
| Veri | Parent `onNeedData(filter, meta)` ile API bağlar |

## Kolon sözleşmesi

- Her kolon için kararlı `key` (veya string `dataIndex`) zorunlu — tercihler buna bağlanır.
- Gizlenemez kolon: `listColumnLocked: true` (veya `fixed: 'right'`).
- Ayarlar drawer’ı ConnectedListPage içinde; ekranlar tekrar yazmaz (`enableColumnSettings`, varsayılan `true`).

## Atagreen sapmaları (Arkas ConnectedListPage’den)

- Redux / pin quick-access / Affix / export yok.
- Ham hex tablo teması yok — semantik token + ConfigProvider.
- Yetki: rota `RequireAuth` / `PermissionGuard`; kabuk permission taşımaz.
- Form alanları: `FloatingInput` / `FormFloatingInput`.
- Filtre şeması ≠ satır DTO; TenantId filtreye konmaz.

## Tüketim

```tsx
<ConnectedListPage
  storageKey="system/tenants"
  filterContent={<TenantsFilter />}
  filterSchema={tenantsFilterSchema}
  defaultFilter={tenantsFilterDefaults}
  enableCreate
  columns={[
    { key: 'name', title: t('…'), dataIndex: 'name', sorter: true },
    { key: 'actions', title: t('…'), listColumnLocked: true, render: … },
  ]}
  onDetailPage={(detail) => <TenantCreateModal … />}
/>
```

Abonelik / yan aksiyon çekmeceleri `onDetailPage` dışında kalabilir.

Sıralama alanları backend whitelist’i ile birebir olmalı (ör. tenants: `name`, `slug`, `isActive`, `createdAtUtc`).
