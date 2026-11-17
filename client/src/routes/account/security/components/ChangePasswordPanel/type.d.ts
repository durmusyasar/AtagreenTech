export interface ChangePasswordPanelProps {
  onSuccessRedirectPath?: string
}

export interface ChangePasswordForm {
  currentPassword: string
  newPassword: string
  confirmPassword: string
}
