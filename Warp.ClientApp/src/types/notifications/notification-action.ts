/** Represents an action associated with a notification. */
export interface NotificationAction {
    label: string
    title?: string
    onClick: () => void
}
