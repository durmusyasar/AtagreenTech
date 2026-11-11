import { Space, Tag } from 'antd'

/** Kaydın çevirisi bulunan dilleri, sistemin varsayılan dili vurgulanmış olarak gösterir. */
export function TranslationTags({ languages, defaultLanguage }: { languages: string[]; defaultLanguage: string }) {
  return (
    <Space size={4} wrap>
      {languages.map((code) => (
        <Tag key={code} color={code === defaultLanguage ? 'green' : undefined}>
          {code.toUpperCase()}
        </Tag>
      ))}
    </Space>
  )
}
