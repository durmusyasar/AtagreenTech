import type { LoginFormValues } from '@modules/auth/pages/login/type'

/** RHF `defaultValues` — alan adları schema ile birebir. */
export const defaultValues: LoginFormValues = {
  email: '',
  password: '',
  rememberMe: false,
  mfaInput: '',
}
