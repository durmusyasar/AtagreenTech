import { CopyOutlined, SafetyCertificateOutlined } from '@ant-design/icons'
import { useQueryClient } from '@tanstack/react-query'
import {
  Alert,
  App,
  Button,
  Col,
  Flex,
  Form,
  Input,
  QRCode,
  Row,
  Space,
  Tag,
  Typography,
} from 'antd'
import { useState } from 'react'
import { FormErrorAlert } from '@/Components/forms/FormErrorAlert'
import { ApiError } from '@/core/api/apiError'
import { invalidate } from '@/core/api/queryClient'
import { useLanguage } from '@/core/context/useLanguage'
import { useAppMutation } from '@/core/hooks/useAppMutation'
import { useAppQuery } from '@/core/hooks/useAppQuery'
import { queryKeys } from '@/services/queryKeys'
import { accountSecurityService } from '@/services/accountSecurityService'
import { sessionQueries } from '@/services/sessionService'
import type { MfaSetupData } from '@/types/security'
import type { MfaFeedbackState, MfaSetupPanelProps } from '@/routes/account/security/components/MfaSetupPanel/type'

export function MfaSetupPanel({ initialEnabled }: MfaSetupPanelProps = {}) {
  const { t } = useLanguage()
  const { message } = App.useApp()
  const queryClient = useQueryClient()
  const me = useAppQuery(sessionQueries.current())

  const [setupData, setSetupData] = useState<MfaSetupData | null>(null)
  const [showDisableForm, setShowDisableForm] = useState(false)
  const [banner, setBanner] = useState<MfaFeedbackState>()
  const [formError, setFormError] = useState<string>()

  const isMfaEnabled = initialEnabled ?? me.data?.isMfaEnabled ?? false

  const refreshMe = () => invalidate(queryClient, queryKeys.session)

  const startSetup = useAppMutation({
    mutationFn: () => accountSecurityService.setupMfa(),
    onSuccess: (data) => {
      setSetupData(data)
      setBanner(undefined)
      setFormError(undefined)
    },
    onError: (error) => {
      setBanner({
        type: 'error',
        text: error instanceof ApiError ? error.message : t('security.mfaSetupFailed'),
      })
    },
  })

  const patchMfaEnabled = (enabled: boolean) => {
    queryClient.setQueryData(sessionQueries.current().queryKey, (current: typeof me.data) =>
      current ? { ...current, isMfaEnabled: enabled } : current,
    )
  }

  const verify = useAppMutation({
    mutationFn: (code: string) => accountSecurityService.verifyMfa(code),
    onSuccess: async () => {
      setSetupData(null)
      setBanner({ type: 'success', text: t('security.mfaEnabled') })
      patchMfaEnabled(true)
      await refreshMe()
    },
    onError: (error) => {
      setFormError(error instanceof ApiError ? error.message : t('security.mfaVerifyFailed'))
    },
  })

  const disable = useAppMutation({
    mutationFn: (password: string) => accountSecurityService.disableMfa(password),
    onSuccess: async () => {
      setShowDisableForm(false)
      setFormError(undefined)
      setBanner({ type: 'success', text: t('security.mfaDisabled') })
      patchMfaEnabled(false)
      await refreshMe()
    },
    onError: (error) => {
      setFormError(error instanceof ApiError ? error.message : t('security.mfaDisableFailed'))
    },
  })

  const copyRecoveryCodes = async () => {
    if (!setupData) return
    try {
      await navigator.clipboard.writeText(setupData.recoveryCodes.join('\n'))
      message.success(t('security.recoveryCopied'))
    } catch {
      message.error(t('security.recoveryCopyFailed'))
    }
  }

  return (
    <Space orientation="vertical" size="middle" style={{ width: '100%' }}>
      <Flex justify="space-between" align="flex-start" wrap gap={12}>
        <div>
          <Typography.Title level={4} style={{ margin: 0 }}>
            {t('security.mfaTitle')}
          </Typography.Title>
          <Typography.Text type="secondary">{t('security.mfaDescription')}</Typography.Text>
        </div>
        <Tag color={isMfaEnabled ? 'success' : 'default'} icon={<SafetyCertificateOutlined />}>
          {isMfaEnabled ? t('list.active') : t('security.mfaOff')}
        </Tag>
      </Flex>

      {banner && (
        <Alert
          type={banner.type}
          showIcon
          closable
          message={banner.text}
          onClose={() => setBanner(undefined)}
        />
      )}

      <FormErrorAlert message={formError} onClose={() => setFormError(undefined)} />

      {!isMfaEnabled && !setupData && (
        <Button
          type="primary"
          loading={startSetup.isPending || me.isPending}
          onClick={() => startSetup.mutate()}
        >
          {startSetup.isPending ? t('security.mfaPreparing') : t('security.mfaStart')}
        </Button>
      )}

      {!isMfaEnabled && setupData && (
        <Space orientation="vertical" size="large" style={{ width: '100%' }}>
          <Row gutter={[16, 16]}>
            <Col xs={24} md={12}>
              <Typography.Title level={5}>{t('security.mfaPairTitle')}</Typography.Title>
              <Typography.Paragraph type="secondary">{t('security.mfaPairHint')}</Typography.Paragraph>
              <QRCode value={setupData.qrCodeUri} size={160} />
              <Typography.Text type="secondary" style={{ display: 'block', marginTop: 12 }}>
                {t('security.mfaSharedKey')}
              </Typography.Text>
              <Typography.Text code copyable style={{ fontSize: 13 }}>
                {setupData.sharedKey}
              </Typography.Text>
            </Col>

            <Col xs={24} md={12}>
              <Flex justify="space-between" align="center" style={{ marginBottom: 8 }}>
                <Typography.Title level={5} style={{ margin: 0 }}>
                  {t('security.mfaRecoveryTitle')}
                </Typography.Title>
                <Button type="link" icon={<CopyOutlined />} onClick={() => void copyRecoveryCodes()}>
                  {t('security.mfaRecoveryCopy')}
                </Button>
              </Flex>
              <Typography.Paragraph type="secondary">{t('security.mfaRecoveryHint')}</Typography.Paragraph>
              <Row gutter={[8, 8]}>
                {setupData.recoveryCodes.map((code) => (
                  <Col span={12} key={code}>
                    <Typography.Text
                      code
                      style={{ display: 'block', textAlign: 'center', padding: '6px 0' }}
                    >
                      {code}
                    </Typography.Text>
                  </Col>
                ))}
              </Row>
            </Col>
          </Row>

          <Form
            layout="vertical"
            requiredMark={false}
            style={{ maxWidth: 360 }}
            onFinish={(values: { code: string }) => {
              setFormError(undefined)
              verify.mutate(values.code)
            }}
          >
            <Form.Item
              name="code"
              label={t('security.mfaVerifyLabel')}
              rules={[
                { required: true, whitespace: true, message: t('login.mfaCodeRequired') },
                { pattern: /^\d{6}$/, message: t('login.mfaTotpInvalid') },
              ]}
            >
              <Input
                maxLength={6}
                autoComplete="one-time-code"
                placeholder="123456"
                style={{ letterSpacing: 4, textAlign: 'center', fontFamily: 'monospace' }}
              />
            </Form.Item>
            <Button type="primary" htmlType="submit" loading={verify.isPending}>
              {t('security.mfaVerifySubmit')}
            </Button>
          </Form>
        </Space>
      )}

      {isMfaEnabled && (
        <>
          {!showDisableForm ? (
            <Button danger onClick={() => setShowDisableForm(true)}>
              {t('security.mfaDisable')}
            </Button>
          ) : (
            <Form
              layout="vertical"
              requiredMark={false}
              style={{ maxWidth: 360 }}
              onFinish={(values: { password: string }) => {
                if (disable.isPending) return
                setFormError(undefined)
                disable.mutate(values.password)
              }}
            >
              <Typography.Paragraph type="secondary">{t('security.mfaDisableHint')}</Typography.Paragraph>
              <Form.Item
                name="password"
                label={t('security.currentPassword')}
                rules={[{ required: true, whitespace: true, message: t('login.passwordRequired') }]}
              >
                <Input.Password autoComplete="current-password" />
              </Form.Item>
              <Space>
                <Button htmlType="button" onClick={() => setShowDisableForm(false)}>
                  {t('common.cancel')}
                </Button>
                <Button type="primary" danger htmlType="submit" loading={disable.isPending}>
                  {t('security.mfaDisableConfirm')}
                </Button>
              </Space>
            </Form>
          )}
        </>
      )}
    </Space>
  )
}
