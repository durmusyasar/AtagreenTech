import { Suspense, type ReactNode } from 'react'
import type { RouteObject } from 'react-router-dom'
import { PageLoader } from '@core/routing/PageLoader'
import { lazyNamed } from '@core/routing/lazyNamed'

const LoginPage = lazyNamed(() => import('@modules/auth/pages/login'), 'LoginPage')
const ForgotPasswordPage = lazyNamed(
  () => import('@modules/auth/pages/forgot-password'),
  'ForgotPasswordPage',
)
const ResetPasswordPage = lazyNamed(
  () => import('@modules/auth/pages/reset-password'),
  'ResetPasswordPage',
)
const AcceptInvitePage = lazyNamed(
  () => import('@modules/auth/pages/accept-invite'),
  'AcceptInvitePage',
)
const VerifyEmailPage = lazyNamed(
  () => import('@modules/auth/pages/verify-email'),
  'VerifyEmailPage',
)

function withSuspense(page: ReactNode) {
  return <Suspense fallback={<PageLoader />}>{page}</Suspense>
}

/** Public auth route’ları — URL path’leri değişmez. */
export function getAuthRoutes(): RouteObject[] {
  return [
    { path: '/login', element: withSuspense(<LoginPage />) },
    { path: '/forgot-password', element: withSuspense(<ForgotPasswordPage />) },
    { path: '/reset-password', element: withSuspense(<ResetPasswordPage />) },
    { path: '/accept-invite', element: withSuspense(<AcceptInvitePage />) },
    { path: '/verify-email', element: withSuspense(<VerifyEmailPage />) },
  ]
}
