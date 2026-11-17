export interface MfaSetupPanelProps {
  initialEnabled?: boolean
}

export interface MfaFeedbackState {
  type: 'success' | 'error'
  text: string
}
