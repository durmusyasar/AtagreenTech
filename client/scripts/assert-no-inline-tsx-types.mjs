/**
 * Mimari kural (#52 / #111) — ESLint `no-restricted-syntax` eşdeğeri.
 *
 * TypeScript 7 ile `typescript-eslint` / compiler API yüzeyi henüz uygun olmadığı için
 * aynı kural burada kaynak taraması ile doğrulanır. `eslint.config.js` kural metninin kaynağıdır.
 */
import fs from 'node:fs'
import path from 'node:path'
import { fileURLToPath } from 'node:url'

const root = path.join(path.dirname(fileURLToPath(import.meta.url)), '..', 'src')

const INTERFACE_RE = /^(?<indent>\s*)(?:export\s+)?interface\s+(?<name>[A-Za-z_][A-Za-z0-9_]*)/gm
const TYPE_RE = /^(?<indent>\s*)(?:export\s+)?type\s+(?<name>[A-Za-z_][A-Za-z0-9_]*)\s*=/gm

function walk(dir, acc = []) {
  for (const entry of fs.readdirSync(dir, { withFileTypes: true })) {
    const full = path.join(dir, entry.name)
    if (entry.isDirectory()) walk(full, acc)
    else if (entry.name.endsWith('.tsx')) acc.push(full)
  }
  return acc
}

/** Basit blok yorum / satır yorum maskesi — yanlış pozitifleri azaltır. */
function stripComments(source) {
  return source
    .replace(/\/\*[\s\S]*?\*\//g, (block) => block.replace(/[^\n]/g, ' '))
    .replace(/(^|[^:])\/\/.*$/gm, '$1')
}

let failed = false

for (const file of walk(root)) {
  const source = stripComments(fs.readFileSync(file, 'utf8'))
  const relative = path.relative(process.cwd(), file)

  for (const match of source.matchAll(INTERFACE_RE)) {
    const line = source.slice(0, match.index).split('\n').length
    console.error(
      `${relative}:${line}: Mimari Kural (#52): .tsx dosyaları içinde interface tanımlamayın. İlgili bileşen klasörü altındaki type.d.ts dosyasını kullanın. (bulunan: interface ${match.groups.name})`,
    )
    failed = true
  }

  for (const match of source.matchAll(TYPE_RE)) {
    const line = source.slice(0, match.index).split('\n').length
    console.error(
      `${relative}:${line}: Mimari Kural (#52): .tsx dosyaları içinde type tanımlamayın. İlgili bileşen klasörü altındaki type.d.ts dosyasını kullanın. (bulunan: type ${match.groups.name})`,
    )
    failed = true
  }
}

if (failed) process.exit(1)

console.log('assert-no-inline-tsx-types: ok')
