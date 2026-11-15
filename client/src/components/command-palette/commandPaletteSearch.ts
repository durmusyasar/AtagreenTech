/**
 * Komut listesinde anlık arama. Tüm boşlukla ayrılmış parçalar başlık / kategori / anahtar
 * kelimelerde geçmelidir (basit fuzzy / token eşleşmesi).
 */
export function filterByQuery<T extends { searchText: string }>(items: readonly T[], query: string): T[] {
  const tokens = query
    .trim()
    .toLowerCase()
    .split(/\s+/)
    .filter(Boolean)

  if (tokens.length === 0) return [...items]

  return items.filter((item) => {
    const haystack = item.searchText.toLowerCase()
    return tokens.every((token) => haystack.includes(token))
  })
}

/** macOS / iOS’ta ⌘K, diğerlerinde Ctrl+K. */
export function commandPaletteShortcutLabel(platform = typeof navigator !== 'undefined' ? navigator.platform : ''): string {
  return /Mac|iPhone|iPad|iPod/i.test(platform) ? '⌘K' : 'Ctrl+K'
}

export function isCommandPaletteToggle(event: KeyboardEvent): boolean {
  return (event.ctrlKey || event.metaKey) && !event.altKey && event.key.toLowerCase() === 'k'
}
