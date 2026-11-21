# ConnectedListPage (liste kabuğu)

Paylaşılan liste UX’i: `client/src/Components/ListPage/`.

## Sorumluluklar

| Özellik | Davranış |
|---|---|
| Arama | `FloatingInput`; Enter / ikon → `filter.search` |
| Filtre temizle | Arama + sıralama sıfır; persistence silinir |
| Filtre kaydet | `localStorage` (`atagreen.listPage.<storageKey>`) |
| Sayfalama | Ant Design Table; `pageSize` değiştirilebilir |
| Sıralama | Sütun `sorter` + `sortOrder`; meta `sorter.field/order` |
| Veri | Parent `onNeedData(filter, meta)` ile API bağlar |

## Atagreen sapmaları (Arkas ConnectedListPage’den)

- Redux / pin quick-access / Affix / export / column settings yok (ihtiyaç olursa ayrı eklenir).
- Ham hex tablo teması yok — semantik token + mevcut Ant Design ConfigProvider.
- Yetki: sayfa rotasında `RequireAuth` / `PermissionGuard`; liste kabuğu permission hook’u taşımaz.
- Form arama: `FloatingInput` (standart Ant `Input.Search` değil).

## Tüketim

```tsx
<ConnectedListPage
  storageKey="system/tenants"
  onNeedData={(filter, meta) => setQuery({ ...filter, ...meta })}
  columns={[
    { title: '…', dataIndex: 'name', sorter: true, sortOrder: sortOrderForField(meta.sorter, 'name') },
  ]}
  …
/>
```

Sıralama alanları backend whitelist’i ile birebir olmalı (ör. tenants: `name`, `slug`, `isActive`, `createdAtUtc`).
