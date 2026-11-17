/**
 * Mimari kural (#52 / #111): bileşen .tsx dosyalarında inline interface/type yok.
 * Props ve yerel tip alias'ları ilgili klasördeki type.d.ts içinde tanımlanır.
 *
 * Hedef ESLint no-restricted-syntax seçicileri:
 * - TSInterfaceDeclaration
 * - TSTypeAliasDeclaration
 *
 * Mesajlar:
 * - Mimari Kural (#52): .tsx dosyaları içinde interface tanımlamayın. İlgili bileşen klasörü altındaki type.d.ts dosyasını kullanın.
 * - Mimari Kural (#52): .tsx dosyaları içinde type tanımlamayın. İlgili bileşen klasörü altındaki type.d.ts dosyasını kullanın.
 *
 * Dosya kapsamı: src altındaki tüm .tsx dosyaları.
 *
 * Not: Proje TypeScript 7 kullandığı için typescript-eslint henüz çalıştırılamıyor.
 * Zorlama: node scripts/assert-no-inline-tsx-types.mjs (npm run lint).
 */
export default []
