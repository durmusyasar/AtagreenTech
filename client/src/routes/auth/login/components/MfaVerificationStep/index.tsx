import { SafetyCertificateOutlined } from '@ant-design/icons'
import { Button, Flex, Form, Input } from 'antd'
import { useLanguage } from '@/core/context/useLanguage'
import type { MfaVerificationStepProps } from '@/routes/auth/login/type'

/** Login MFA ikinci adım alanları (TOTP / kurtarma kodu); hatalar Zod'dan gelir. */
export function MfaVerificationStep({ mode, onToggleMode, onBack, error }: MfaVerificationStepProps) {
  const { t } = useLanguage()

  return (
    <>
      <Form.Item
        name="mfaInput"
        validateStatus={error ? 'error' : undefined}
        help={error}
      >
        <Input
          autoFocus
          autoComplete="one-time-code"
          maxLength={mode === 'TOTP' ? 6 : 8}
          placeholder={mode === 'TOTP' ? '000000' : 'A1B2C3D4'}
          prefix={<SafetyCertificateOutlined />}
          className="!rounded-xl !text-center !font-mono !text-lg"
          style={{
            letterSpacing: mode === 'TOTP' ? 6 : 3,
            textTransform: mode === 'RECOVERY' ? 'uppercase' : undefined,
          }}
        />
      </Form.Item>

      <Flex justify="space-between" className="!mb-4">
        <Button type="link" className="!px-0" onClick={onBack}>
          ← {t('login.backToCredentials')}
        </Button>
        <Button type="link" className="!px-0" onClick={onToggleMode}>
          {mode === 'TOTP' ? t('login.useRecoveryCode') : t('login.useTotpCode')}
        </Button>
      </Flex>
    </>
  )
}
