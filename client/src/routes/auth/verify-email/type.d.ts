import type { VerifyEmailFormValues } from '@/routes/auth/verify-email/schema'

export type { VerifyEmailFormValues }

export type VerifyStatus = 'VERIFYING' | 'SUCCESS' | 'ERROR' | 'RESEND'
