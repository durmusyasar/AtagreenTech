import { SafetyCertificateOutlined } from '@ant-design/icons'
import { Button, Flex, Form, Input, Typography } from 'antd'
import { useLanguage } from '@/core/context/useLanguage'
import type { MfaVerificationStepProps } from '@/routes/auth/login/type'

/** Login MFA ikinci adım alanları (TOTP / kurtarma kodu). */
export function MfaVerificationStep({ mode, onToggleMode, onBack }: MfaVerificationStepProps) {
  const { t } = useLanguage()

  return (
    <>
      <Typography.Paragraph type="secondary" style={{ marginBottom: 12 }}>
        {mode === 'TOTP' ? t('login.mfaTotpHint') : t('login.mfaRecoveryHint')}
      </Typography.Paragraph>

      <Form.Item
        name="mfaInput"
        label={mode === 'TOTP' ? t('login.mfaTotpLabel') : t('login.mfaRecoveryLabel')}
        rules={[
          { required: true, whitespace: true, message: t('login.mfaCodeRequired') },
          {
            validator: async (_, value: string | undefined) => {
              const raw = value?.trim() ?? ''
              if (!raw) return
              if (mode === 'TOTP' && !/^\d{6}$/.test(raw)) {
                throw new Error(t('login.mfaTotpInvalid'))
              }
              if (mode === 'RECOVERY' && !/^[A-Za-z0-9]{8}$/.test(raw)) {
                throw new Error(t('login.mfaRecoveryInvalid'))
              }
            },
          },
        ]}
      >
        <Input
          autoFocus
          autoComplete="one-time-code"
          maxLength={mode === 'TOTP' ? 6 : 8}
          placeholder={mode === 'TOTP' ? '000000' : 'A1B2C3D4'}
          prefix={<SafetyCertificateOutlined />}
          style={{
            letterSpacing: mode === 'TOTP' ? 6 : 3,
            textAlign: 'center',
            fontSize: 18,
            fontFamily: 'ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, monospace',
            textTransform: mode === 'RECOVERY' ? 'uppercase' : undefined,
          }}
        />
      </Form.Item>

      <Flex justify="space-between" style={{ marginBottom: 16 }}>
        <Button type="link" style={{ paddingInline: 0 }} onClick={onBack}>
          ← {t('login.backToCredentials')}
        </Button>
        <Button type="link" style={{ paddingInline: 0 }} onClick={onToggleMode}>
          {mode === 'TOTP' ? t('login.useRecoveryCode') : t('login.useTotpCode')}
        </Button>
      </Flex>
    </>
  )
}
