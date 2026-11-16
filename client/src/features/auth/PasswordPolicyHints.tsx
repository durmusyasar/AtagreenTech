import { CheckCircleOutlined, CloseCircleOutlined } from '@ant-design/icons'
import { Flex, Typography } from 'antd'
import { useLanguage } from '../../core/context/useLanguage'
import { checkPasswordStrength } from '../../core/passwordPolicy'

interface PasswordPolicyHintsProps {
  password: string
}

/** Backend `PasswordRules` ile uyumlu canlı şifre kuralı listesi. */
export function PasswordPolicyHints({ password }: PasswordPolicyHintsProps) {
  const { t } = useLanguage()
  const policy = checkPasswordStrength(password)

  const rows: Array<{ ok: boolean; label: string }> = [
    { ok: policy.minLength, label: t('password.ruleMinLength') },
    { ok: policy.hasUpper && policy.hasLower, label: t('password.ruleCase') },
    { ok: policy.hasDigit && policy.hasSpecial, label: t('password.ruleDigitSpecial') },
  ]

  return (
    <Flex vertical gap={4} style={{ marginTop: 8 }}>
      {rows.map((row) => (
        <Typography.Text
          key={row.label}
          type={row.ok ? 'success' : 'secondary'}
          style={{ fontSize: 12 }}
        >
          {row.ok ? <CheckCircleOutlined /> : <CloseCircleOutlined />} {row.label}
        </Typography.Text>
      ))}
    </Flex>
  )
}
