import { Space, Tag } from 'antd'

/** Kaydın çevirisi bulunan dilleri, sistemin varsayılan dili başta ve vurgulanmış olarak gösterir. */
export function TranslationTags({ languages, defaultLanguage }: { languages: string[]; defaultLanguage: string }) {
  const ordered = [...languages].sort((a, b) => Number(b === defaultLanguage) - Number(a === defaultLanguage))

  return (
    <Space size={4} wrap>
      {ordered.map((code) => (
        <Tag key={code} color={code === defaultLanguage ? 'green' : undefined}>
          {code.toUpperCase()}
        </Tag>
      ))}
    </Space>
  )
}
